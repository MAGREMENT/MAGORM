using System.Collections.Generic;
using System.Text;
using System.Threading;
using GeneratorCommon;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace BaseGenerator;

public static class ExtensibleGenerator
{
    private const string ExtensibleClassAttributeFullName = "Base.Extensibility.ExtensibleClassAttribute";
    private const string ExtensibleMethodAttributeFullName = "Base.Extensibility.ExtensibleMethodAttribute";

    public static void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var declarations = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                ExtensibleClassAttributeFullName,
                IsSyntaxTargetForGeneration,
                GetSemanticTargetForGeneration)
            .Where(GeneratorHelper.IsSyntaxNotNull);

        var declarationsWithCompiler = declarations.Combine(context.CompilationProvider);
        context.RegisterSourceOutput(declarationsWithCompiler, (a, b) => GenerateSources(a, b.Left, b.Right));
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node, CancellationToken token)
        => node is ClassDeclarationSyntax;

    private static ExtensibleClassAttributes? GetSemanticTargetForGeneration(GeneratorAttributeSyntaxContext context,
        CancellationToken token)
    {
        foreach (var attr in context.Attributes)
        {
            if (attr.AttributeClass?.ToDisplayString() != ExtensibleClassAttributeFullName) continue;

            var ctorArgs = attr.ConstructorArguments;
            return new((INamedTypeSymbol)context.TargetSymbol,
                (string)ctorArgs[0].Value!,
                (string)ctorArgs[1].Value!);
        }

        return null;
    }

    private static void GenerateSources(SourceProductionContext context, ExtensibleClassAttributes? attr,
        Compilation compilation)
    {
        if (attr is null) return;

        var extensibleMethods = new List<ExtensibleMethodAttributes>();
        foreach (var method in attr.Symbol.GetMembers().OfType<IMethodSymbol>())
        {
            foreach (var a in method.GetAttributes())
            {
                if (a.AttributeClass?.ToDisplayString() == ExtensibleMethodAttributeFullName)
                {
                    extensibleMethods.Add(new ExtensibleMethodAttributes(method,
                        a.ConstructorArguments[0].Value as string));
                    break;
                }
            }
        }

        var collectionName = $"IExtensionCollection<{attr.ExtensionName}, int>";
        var builder = new StringBuilder();
        
        var sourceHeader = $"""
        using Base.Extensibility;
        using System.Reflection;
        
        namespace {attr.Symbol.ContainingNamespace.ToDisplayString()};
        
        """;
        builder.Append(sourceHeader);
        
        CreateBaseStruct(builder, attr.BaseName, attr.Symbol.Name, collectionName, extensibleMethods);
        CreateExtensibleClass(builder, attr.Symbol.Name, collectionName, attr.BaseName, extensibleMethods);
        CreateExtensionClass(builder, attr.ExtensionName, attr.BaseName, extensibleMethods);

        context.AddSource($"{attr.Symbol.Name}.g.cs", SourceText.From(builder.ToString(), Encoding.UTF8));
    }

    private static void CreateExtensibleClass(StringBuilder builder, string extensibleName, string collectionName,
        string baseName, IReadOnlyList<ExtensibleMethodAttributes> methodList)
    {
        var methodBuilder = new StringBuilder();
        foreach (var method in methodList)
        {
            CreateExtensibleMethod(methodBuilder, method, baseName);
        }
        
        var result = $$"""
        public partial class {{extensibleName}}
        {
            private {{collectionName}}? _extensionCollection;
            
            public void SetExtensionCollection({{collectionName}}? collection) {
                _extensionCollection = collection;
            }
            
        {{methodBuilder}}
        }
        
        """;
        builder.Append(result);
    }

    private static void CreateExtensibleMethod(StringBuilder builder, ExtensibleMethodAttributes method, string baseName)
    {
        GeneratorHelper.CreateMethodSignature(builder, method.Symbol, null, "partial");
        builder.Append("\t\tif(_extensionCollection is null) ");
        if (method.BaseImplementation is not null)
        {
            if (!method.Symbol.ReturnsVoid) builder.Append("return ");
            builder.Append(method.BaseImplementation);
            builder.Append('(');
            for (int i = 0; i < method.Symbol.Parameters.Length; i++)
            {
                if (i > 0) builder.Append(", ");
                builder.Append(method.Symbol.Parameters[i].Name);
            }

            builder.Append(");\n");
        }
        else
        {
            builder.Append("throw new MissingBehaviorException(nameof(");
            builder.Append(method.Symbol.Name);
            builder.Append("));\n");
        }

        builder.Append("\t\telse ");
        if (!method.Symbol.ReturnsVoid) builder.Append("return ");
        builder.Append("new ");
        builder.Append(baseName);
        builder.Append("(this, _extensionCollection, _extensionCollection.BaseState).");
        builder.Append(method.Symbol.Name);
        builder.Append('(');
        for (int i = 0; i < method.Symbol.Parameters.Length; i++)
        {
            if (i > 0) builder.Append(", ");
            builder.Append(method.Symbol.Parameters[i].Name);
        }
        
        builder.Append(");\n\t}\n");
    }

    private static void CreateBaseStruct(StringBuilder builder, string baseName, string extendedName, string collectionName,
        IReadOnlyList<ExtensibleMethodAttributes> methodList)
    {
        var methodBuilder = new StringBuilder();
        for(int i = 0; i < methodList.Count; i++)
        {
            CreateBaseMethod(methodBuilder, methodList[i], i, baseName);
        }

        var result = $$"""
                 public readonly partial struct {{baseName}}
                 {
                     public readonly {{extendedName}} Subject;
                     private readonly {{collectionName}} _collection;
                     private readonly int _state;
                           
                     public {{baseName}}({{extendedName}} subject, {{collectionName}} collection, int state) {
                         Subject = subject;
                         _collection = collection;
                         _state = state;
                     }
                           
                 {{methodBuilder}}
                 }
                 
                 """;
        builder.Append(result);
    }

    private static void CreateBaseMethod(StringBuilder builder, ExtensibleMethodAttributes method, int identifier, string baseName)
    {
        var symbol = method.Symbol;
        GeneratorHelper.CreateMethodSignature(builder, symbol);
        builder.Append("\t\tvar next = _collection.Next(_state, ");
        builder.Append(identifier);
        builder.Append(");\n");
        builder.Append("\t\tif(next is not null) ");
        if (!symbol.ReturnsVoid) builder.Append("return ");
        builder.Append("next.");
        builder.Append(symbol.Name);
        builder.Append('(');
        int i = 0;
        for (; i < symbol.Parameters.Length; i++)
        {
            if (i > 0) builder.Append(", ");
            builder.Append(symbol.Parameters[i].Name);
        }
        if (i > 0) builder.Append(", ");
        builder.Append("new ");
        builder.Append(baseName);
        builder.Append("(Subject, _collection, _state + 1)");

        builder.Append(");\n");
        builder.Append("\t\telse ");
        if (method.BaseImplementation is not null)
        {
            if (!symbol.ReturnsVoid) builder.Append("return ");
            builder.Append("Subject.");
            builder.Append(method.BaseImplementation);
            builder.Append('(');
            for (i = 0; i < symbol.Parameters.Length; i++)
            {
                if(i > 0) builder.Append(", ");
                builder.Append(symbol.Parameters[i].Name);
            }
            builder.Append(");\n");
        }
        else
        {
            if (symbol.ReturnsVoid) builder.Append("if (_state == _collection.BaseState) ");
            builder.Append("throw new MissingBehaviorException(nameof(");
            builder.Append(method.Symbol.Name);
            builder.Append("));\n");
        }
        
        builder.Append("\t}\n\n");
    }

    private static void CreateExtensionClass(StringBuilder builder, string extensionName, string baseName,
        IReadOnlyList<ExtensibleMethodAttributes> methodList)
    {
        var methodBuilder = new StringBuilder();
        var switchBuilder = new StringBuilder();
        for(int i = 0; i < methodList.Count; i++)
        {
            CreateExtensionMethod(methodBuilder, methodList[i].Symbol, baseName);
            switchBuilder.Append("\t\t\t\"");
            switchBuilder.Append(methodList[i].Symbol.Name);
            switchBuilder.Append("\" => ");
            switchBuilder.Append(i);
            switchBuilder.Append(",\n");
        }
        
        var result = $$"""
        public partial class {{extensionName}} : DynamicReflectiveExtension
        {
        
            protected override int GetMethodNumber(MethodInfo method) {
                return method.Name switch
                {
        {{switchBuilder}}
                    _ => -1
                };
            }
        
        {{methodBuilder}}
        }               
        """;
        builder.Append(result);
    }

    private static void CreateExtensionMethod(StringBuilder builder, IMethodSymbol symbol, string baseName)
    {
        //TODO make additional parameters functionality better
        GeneratorHelper.CreateMethodSignature(builder, symbol, b =>
        {
            b.Append(", ");
            b.Append(baseName);
            b.Append(' ');
            b.Append("previous");
        }, "virtual");
        if (!symbol.ReturnsVoid) builder.Append("\t\treturn default;\n");
        builder.Append("\t}\n");
    }
    
    private static readonly DiagnosticDescriptor BaseMethodNotFound = new(
        id: "EXT003",
        title: "Base Method Not Found",
        messageFormat: "The method {0} has a different return type than void but does not have a base implementation",
        category: "SourceGeneration",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}

public class ExtensibleClassAttributes(INamedTypeSymbol Symbol, string ExtensionName, string BaseName)
{
    public readonly INamedTypeSymbol Symbol = Symbol;
    public readonly string ExtensionName = ExtensionName;
    public readonly string BaseName = BaseName;
}

public class ExtensibleMethodAttributes(IMethodSymbol Symbol, string? BaseImplementation)
{
    public readonly IMethodSymbol Symbol = Symbol;
    public readonly string? BaseImplementation = BaseImplementation;
}