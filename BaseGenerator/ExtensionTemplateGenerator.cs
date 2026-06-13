using System;
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
            .Where(Generator.IsValidSyntax);

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

        var extendedSymbol = compilation.GetTypeByMetadataName(attr.Symbol.ContainingNamespace.ToDisplayString() 
                                                               + "." + attr.ExtensionName);
        
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
                context.ReportDiagnostic(Diagnostic.Create(InvalidTemplate, 
                    method.Locations.FirstOrDefault(),
                    method.Name,
                    attr.BaseName));
            }

            CreateCollectionMethod(collectionBuilder, method, attr.ExtensionName, attr.BaseName);
            CreateBaseMethod(baseBuilder, method);
        }

        var collectionType = attr.ExtensionName + "ExtensionCollection";
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

    private static void CreateCollectionMethod(StringBuilder builder, IMethodSymbol symbol, string? extendedName, string? baseName)
    {
        CreateMethodSignature(builder, symbol, b =>
        {
            b.Append(", ");
            b.Append(extendedName);
            b.Append(" subject, int state");
        });

        builder.Append("\t\tif(state < Count) this[state].");
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
        
        builder.Append("\t}\n\n");
    }

    private static void CreateBaseMethod(StringBuilder builder, IMethodSymbol symbol)
    {
        CreateMethodSignature(builder, symbol);
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

    private static void CreateMethodSignature(StringBuilder builder, IMethodSymbol symbol, Action<StringBuilder>? additionalParameters = null)
    {
        builder.Append("\tpublic ");
        builder.Append(symbol.ReturnType.ToDisplayString());
        builder.Append(' ');
        builder.Append(symbol.Name);
        builder.Append('(');
        for (int i = 1; i < symbol.Parameters.Length; i++)
        {
            if (i != 1) builder.Append(", ");
            builder.Append(symbol.Parameters[i].Type.ToDisplayString());
            builder.Append(' ');
            builder.Append(symbol.Parameters[i].Name);
        }
        if(additionalParameters is not null) additionalParameters(builder);

        builder.Append(") {\n");
    }
    
    private static readonly DiagnosticDescriptor InvalidTemplate = new(
        id: "EXT001",
        title: "Invalid Extension Template Function",
        messageFormat: "The function {0} needs to start with a parameter of type {1}",
        category: "SourceGeneration",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}

internal record ExtensionTemplateAttributes(INamedTypeSymbol Symbol, string? ExtensionName, string? BaseName);