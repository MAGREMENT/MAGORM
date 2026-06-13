using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace BaseGenerator;

public static class ExtensibleGenerator //TODO generalize the pipeline a bit, a lot of copy & paste right now
{
    public static void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var declarations = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "Base.Extensibility.ExtensibleAttribute",
                IsSyntaxTargetForGeneration,
                GetSemanticTargetForGeneration)
            .Where(GeneratorHelper.IsValidSyntax);

        var declarationsWithCompiler = declarations.Combine(context.CompilationProvider);
        context.RegisterSourceOutput(declarationsWithCompiler, (a, b) => GenerateSources(a, b.Left, b.Right));
    }
    
    private static bool IsSyntaxTargetForGeneration(SyntaxNode node, CancellationToken token)
        => node is ClassDeclarationSyntax;

    private static ExtensibleAttributes? GetSemanticTargetForGeneration(GeneratorAttributeSyntaxContext context,
        CancellationToken token)
    {
        foreach (var attr in context.Attributes)
        {
            if(attr.AttributeClass?.ToDisplayString() != "Base.Extensibility.ExtensibleAttribute") continue;

            var ctorArgs = attr.ConstructorArguments;
            return new((INamedTypeSymbol)context.TargetSymbol, ctorArgs[0].Values
                .Select(v => (string)v.Value!)
                .ToArray());
        }
        
        return null;
    }

    private static void GenerateSources(SourceProductionContext context, ExtensibleAttributes? attr,
        Compilation compilation)
    {
        if (attr is null) return;
        
        var baseMethods = ExtensibilityGeneratorHelper.GetBaseMethods(attr.Symbol);

        var collectionDeclarationsBuilder = new StringBuilder();
        var setExtensionBuilder = new StringBuilder();
        var extensionMethodsBuilder = new StringBuilder();
        var n = 1;
        foreach (var extension in attr.Extensions)
        {
            var fullInterfaceName = attr.Symbol.ContainingNamespace.ToDisplayString() + "." + extension;
            var wantedInterface = compilation.GetTypeByMetadataName(fullInterfaceName);
            if (wantedInterface is null) continue;
            
            collectionDeclarationsBuilder.Append("\n\tprivate IExtensionCollection<");
            collectionDeclarationsBuilder.Append(extension);
            collectionDeclarationsBuilder.Append(">? _extensionCollection");
            collectionDeclarationsBuilder.Append(n);
            collectionDeclarationsBuilder.Append(';');

            setExtensionBuilder.Append("\t\tif (tType == typeof(");
            setExtensionBuilder.Append(extension);
            setExtensionBuilder.Append("))\n\t\t{\n\t\t\t");
            setExtensionBuilder.Append("_extensionCollection");
            setExtensionBuilder.Append(n);
            setExtensionBuilder.Append(" = (IExtensionCollection<");
            setExtensionBuilder.Append(extension);
            setExtensionBuilder.Append(">)collection;\n\t\t\treturn;\n\t\t}");

            foreach (var method in wantedInterface.GetMembers().OfType<IMethodSymbol>())
            {
                if(method.MethodKind != MethodKind.Ordinary) continue;
                
                GeneratorHelper.CreateMethodSignature(extensionMethodsBuilder, method);
                extensionMethodsBuilder.Append("\t\tif(_extensionCollection");
                extensionMethodsBuilder.Append(n);
                extensionMethodsBuilder.Append(" is null) ");
                if (baseMethods.TryGetValue(method.Name, out var baseMethod))
                {
                    if (!method.ReturnsVoid) extensionMethodsBuilder.Append("return ");
                    extensionMethodsBuilder.Append(baseMethod.Name);
                    extensionMethodsBuilder.Append('(');
                    for (int i = 1; i < method.Parameters.Length; i++)
                    {
                        if(i > 1) extensionMethodsBuilder.Append(", ");
                        extensionMethodsBuilder.Append(method.Parameters[i].Name);
                    }
                    extensionMethodsBuilder.Append(");\n");
                }
                else
                {
                    extensionMethodsBuilder.Append("return");
                    if (!method.ReturnsVoid) extensionMethodsBuilder.Append(" default");
                    extensionMethodsBuilder.Append(";\n");
                }

                extensionMethodsBuilder.Append("\t\telse ");
                extensionMethodsBuilder.Append("_extensionCollection");
                extensionMethodsBuilder.Append(n);
                extensionMethodsBuilder.Append('.');
                extensionMethodsBuilder.Append(method.Name);
                extensionMethodsBuilder.Append('(');
                for (int i = 1; i < method.Parameters.Length; i++)
                {
                    if(i > 1) extensionMethodsBuilder.Append(", ");
                    extensionMethodsBuilder.Append(method.Parameters[i].Name);
                }

                extensionMethodsBuilder.Append(", this, ");
                extensionMethodsBuilder.Append("_extensionCollection");
                extensionMethodsBuilder.Append(n);
                extensionMethodsBuilder.Append(".BaseState);\n\t}\n");
            }
        }
        var finalSource = $$"""
                            using Base.Extensibility;
                            
                            namespace {{attr.Symbol.ContainingNamespace.ToDisplayString()}};
                            
                            public partial class {{attr.Symbol.Name}} 
                            {
                            {{collectionDeclarationsBuilder}}
                            
                                public void SetExtensionCollection<T>(IExtensionCollection<T> collection) {
                                    var tType = typeof(T);
                            {{setExtensionBuilder}}
                                }
                                
                            {{extensionMethodsBuilder}}
                            }
                            """;
        
        context.AddSource($"{attr.Symbol.Name}.g.cs", SourceText.From(finalSource, Encoding.UTF8));
    }
}

public record ExtensibleAttributes(INamedTypeSymbol Symbol, string[] Extensions);