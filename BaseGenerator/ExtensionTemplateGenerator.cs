using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace BaseGenerator;

public static class ExtensionTemplateGenerator
{
    public static void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var declarations = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "Base.Extensibility.ExtensionTemplateAttribute",
                IsSyntaxTargetForGeneration,
                GetSemanticTargetForGeneration)
            .Where(GeneratorHelper.IsValidSyntax);

        var declarationsWithCompiler = declarations.Combine(context.CompilationProvider);
        context.RegisterSourceOutput(declarationsWithCompiler, (a, b) => GenerateSources(a, b.Left, b.Right));
    }
    
    private static bool IsSyntaxTargetForGeneration(SyntaxNode node, CancellationToken token)
        => node is InterfaceDeclarationSyntax;

    private static ExtensionTemplateAttributes? GetSemanticTargetForGeneration(GeneratorAttributeSyntaxContext context,
        CancellationToken token)
    {
        foreach (var attr in context.Attributes)
        {
            if(attr.AttributeClass?.ToDisplayString() != "Base.Extensibility.ExtensionTemplateAttribute") continue;

            var ctorArgs = attr.ConstructorArguments;
            return new((INamedTypeSymbol)context.TargetSymbol, ctorArgs[0].Value as string, ctorArgs[1].Value as string);
        }
        
        return null;
    }

    private static void GenerateSources(SourceProductionContext context, ExtensionTemplateAttributes? attr, Compilation compilation)
    {
        if (attr is null) return;

        var fullExtendedClassName = attr.Symbol.ContainingNamespace.ToDisplayString() + "." + attr.ExtensionName;
        var extendedSymbol = compilation.GetTypeByMetadataName(fullExtendedClassName);
        if (extendedSymbol is null)
        {
            context.ReportDiagnostic(Diagnostic.Create(ExtendedClassNotFound, 
                attr.Symbol.Locations.FirstOrDefault(),
                fullExtendedClassName));
            return;
        }

        var baseMethods = ExtensibilityGeneratorHelper.GetBaseMethods(extendedSymbol);
        var methods = attr.Symbol
            .GetMembers()
            .OfType<IMethodSymbol>();

        var baseBuilder = new StringBuilder();
        var collectionBuilder = new StringBuilder();

        foreach (var method in methods)
        {
            if(method.MethodKind != MethodKind.Ordinary) continue;

            if (method.Parameters.Length == 0 || method.Parameters[0].Type.Name != attr.BaseName)
            {
                context.ReportDiagnostic(Diagnostic.Create(InvalidTemplateMethod, 
                    method.Locations.FirstOrDefault(),
                    method.Name,
                    attr.BaseName));
                return;
            }

            if (!CreateCollectionMethod(collectionBuilder, method, attr.ExtensionName, attr.BaseName, baseMethods))
            {
                context.ReportDiagnostic(Diagnostic.Create(BaseMethodNotFound,
                    method.Locations.FirstOrDefault(),
                    method.Name));
            }
            CreateBaseMethod(baseBuilder, method);
        }

        var collectionType = attr.Symbol.Name.Substring(1) + "ExtensionCollection";
        var finalSource = $$"""
                            using Base.Extensibility;
                            
                            namespace {{attr.Symbol.ContainingNamespace.ToDisplayString()}};
                            
                            public partial class {{collectionType}} : ListExtensionCollection<{{attr.Symbol.Name}}>
                            {
                            {{collectionBuilder}}
                            }
                            
                            public partial struct {{attr.BaseName}}
                            {
                                public readonly {{attr.ExtensionName}} Subject;
                                private readonly {{collectionType}} _collection;
                                private readonly int _state;
              
                                public {{attr.BaseName}}({{attr.ExtensionName}} subject, {{collectionType}} collection, int state) {
                                    Subject = subject;
                                    _collection = collection;
                                    _state = state;
                                }
              
                            {{baseBuilder}}
                            }
                            """;
        
         context.AddSource($"{attr.Symbol.Name}.g.cs", SourceText.From(finalSource, Encoding.UTF8));
    }

    private static bool CreateCollectionMethod(StringBuilder builder, IMethodSymbol symbol, string? extendedName, 
        string? baseName, Dictionary<string, IMethodSymbol> baseMethods)
    {
        GeneratorHelper.CreateMethodSignature(builder, symbol, b =>
        {
            b.Append(", ");
            b.Append(extendedName);
            b.Append(" subject, int state");
        });

        builder.Append("\t\tif(state < Count) ");
        if (!symbol.ReturnsVoid) builder.Append("return ");
        builder.Append("this[state].");
        builder.Append(symbol.Name);
        builder.Append("(new ");
        builder.Append(baseName);
        builder.Append("(subject, this, state + 1)");
        for (int i = 1; i < symbol.Parameters.Length; i++)
        {
            builder.Append(", ");
            builder.Append(symbol.Parameters[i].Name);
        }

        builder.Append(");\n");
        if (baseMethods.TryGetValue(symbol.Name, out var baseMethod))
        {
            builder.Append("\t\telse ");
            if (!symbol.ReturnsVoid) builder.Append("return ");
            builder.Append("subject.");
            builder.Append(baseMethod.Name);
            builder.Append('(');
            for (int i = 1; i < symbol.Parameters.Length; i++)
            {
                if(i > 1) builder.Append(", ");
                builder.Append(symbol.Parameters[i].Name);
            }
            builder.Append(");\n");
        }
        else if (!symbol.ReturnsVoid) return false;
        
        builder.Append("\t}\n\n");
        return true;
    }

    private static void CreateBaseMethod(StringBuilder builder, IMethodSymbol symbol)
    {
        GeneratorHelper.CreateMethodSignature(builder, symbol);
        builder.Append("\t\t");
        if (!symbol.ReturnsVoid) builder.Append("return ");
        builder.Append("_collection.");
        builder.Append(symbol.Name);
        builder.Append('(');
        for (int i = 1; i < symbol.Parameters.Length; i++)
        {
            if(i != 1) builder.Append(", ");
            builder.Append(symbol.Parameters[i].Name);
        }

        builder.Append(", Subject, _state);\n");
        builder.Append("\t}\n\n");
    }
    
    private static readonly DiagnosticDescriptor InvalidTemplateMethod = new(
        id: "EXT001",
        title: "Invalid Extension Template Function",
        messageFormat: "The function {0} needs to start with a parameter of type {1}",
        category: "SourceGeneration",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    
    private static readonly DiagnosticDescriptor ExtendedClassNotFound = new(
        id: "EXT002",
        title: "Extended Class Not Found",
        messageFormat: "The extended class {0} was not found",
        category: "SourceGeneration",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
    
    private static readonly DiagnosticDescriptor BaseMethodNotFound = new(
        id: "EXT003",
        title: "Base Method Not Found",
        messageFormat: "The method {0} has a different return type than void but does not have a base implementation",
        category: "SourceGeneration",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}

internal class ExtensionTemplateAttributes(INamedTypeSymbol Symbol, string? ExtensionName, string? BaseName)
{
    public readonly INamedTypeSymbol Symbol = Symbol;
    public readonly string? ExtensionName = ExtensionName;
    public readonly string? BaseName = BaseName;
}