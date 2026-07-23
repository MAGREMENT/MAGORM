using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace BaseGenerator;

public static class PropertyFieldCollectionGenerator
{
    private const string PropertyFieldCollectionAttributeFullName = "Base.Fields.Implementations.PropertyFieldCollectionAttribute";
    private const string FieldAttributeFullName = "Base.Fields.Implementations.FieldAttribute";
    
    public static void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var declarations = context.SyntaxProvider
            .CreateSyntaxProvider(IsSyntaxTargetForGeneration, GetSemanticTargetForGeneration)
            .Where(GeneratorHelper.IsValidSyntax);
        
        context.RegisterSourceOutput(declarations, GenerateSources);
    }
    
    private static bool IsSyntaxTargetForGeneration(SyntaxNode node, CancellationToken token)
        => node is ClassDeclarationSyntax { AttributeLists.Count: > 0 };
    
    private static INamedTypeSymbol? GetSemanticTargetForGeneration(GeneratorSyntaxContext context, CancellationToken token)
    {
        var classSyntax = (ClassDeclarationSyntax)context.Node;
        foreach (var modifier in classSyntax.Modifiers)
        {
            if (modifier.IsKind(SyntaxKind.AbstractKeyword)) return null;
        }
        foreach (var attributeListSyntax in classSyntax.AttributeLists)
        {
            foreach (var attributeSyntax in attributeListSyntax.Attributes)
            {
                var attributeSymbol = ModelExtensions.GetSymbolInfo(context.SemanticModel, attributeSyntax).Symbol;
                if (attributeSymbol?.ContainingType?.ToDisplayString() == PropertyFieldCollectionAttributeFullName)
                {
                    return ModelExtensions.GetDeclaredSymbol(context.SemanticModel, classSyntax) as INamedTypeSymbol;
                }
            }
        }
        
        return null;
    }


    private static void GenerateSources(SourceProductionContext context, INamedTypeSymbol? symbol)
    {
        if (symbol is null) return;

        var builder = new StringBuilder();
        var properties = GeneratorHelper.GetAllPropertiesWithAttribute(symbol, FieldAttributeFullName);
        var sourceHeader = $$"""
        namespace {{symbol.ContainingNamespace.ToDisplayString()}};
        
        public partial class {{symbol.Name}} {
        
            public override int GetFieldCount() {
                return {{properties.Count}};
            }
            
            public override IEnumerable<string> GetFieldsName() {
                 return [{{string.Join(", ", properties.Select(p => $"\"{p.Name}\""))}}];
            }
        
        """;
        builder.Append(sourceHeader);
        builder.Append(GetSetAndGet(properties));
        builder.Append("\n}");
        
        context.AddSource($"{symbol.Name}.g.cs", SourceText.From(builder.ToString(), Encoding.UTF8));
    }
    
    
    private static string GetSetAndGet(IReadOnlyList<IPropertySymbol> properties)
    {
        var setBuilder = new StringBuilder();
        var getBuilder = new StringBuilder();

        foreach (var prop in properties)
        {
            setBuilder.Append("\n\t\t\tcase \"");
            setBuilder.Append(prop.Name);
            setBuilder.Append("\" : ");
            setBuilder.Append(prop.Name);
            setBuilder.Append(" = (");
            setBuilder.Append(prop.Type.ToDisplayString());
            setBuilder.Append(")value; break;");

            getBuilder.Append("\n\t\t\tcase \"");
            getBuilder.Append(prop.Name);
            getBuilder.Append("\" : value = ");
            getBuilder.Append(prop.Name);
            getBuilder.Append("; return true;");
        }
        
        return $$"""
                   public override void Set(string key, object? value)
                   {
                       switch(key) {{{setBuilder}}
                       }
                   }
                   
                   public override bool TryGet(string key, out object? value)
                   {
                       switch(key) {{{getBuilder}}
                           default: value = null; return false;
                       };
                   }
               """;
    }
}