using System.Text;
using GeneratorCommon;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace ORMGenerator;

public static class ModelDefinitionGenerator //TODO
{
    private const string ModelDefinitionAttributeFullName = "ORM.RecordTypes.ModelDefinitionAttribute";
    private const string ModelFieldAttributeFullName = "ORM.RecordTypes.ModelFieldAttribute";
    
    public static void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var declarations = context.SyntaxProvider
            .CreateSyntaxProvider(GeneratorHelper.HasSyntaxTargetAtLeastOneAttribute, 
                GeneratorHelper.GetSemanticTargetNonAbstractClassWithAttribute(ModelDefinitionAttributeFullName))
            .Where(GeneratorHelper.IsSyntaxNotNull);
        
        context.RegisterSourceOutput(declarations, GenerateSources);
    }

    private static void GenerateSources(SourceProductionContext context, INamedTypeSymbol? symbol)
    {
        if (symbol is null) return;

        var builder = new StringBuilder();
        var sourceHeader = $$"""
        using ORM.Abstract;
        using ORM.RecordTypes;
        
        namespace {{symbol.ContainingNamespace.ToDisplayString()}};
        
        public static class {{symbol.Name}}Model {
        
            public static IModel Instance = ModelGenerator.Generate(typeof({{symbol.Name}}));
        
        }

        public partial class {{symbol.Name}} {

            public override IModel GetModel() => {{symbol.Name}}Model.Instance;
        
        }
        """;
        builder.Append(sourceHeader);
        
        context.AddSource($"{symbol.Name}.g.cs", SourceText.From(builder.ToString(), Encoding.UTF8));
    }
}