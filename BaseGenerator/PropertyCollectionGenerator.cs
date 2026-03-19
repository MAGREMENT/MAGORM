using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace BaseGenerator;

[Generator]
public class PropertyCollectionGenerator : IIncrementalGenerator
{
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(IsSyntaxTargetForGeneration, GetSemanticTargetForGeneration)
            .Where(IsValidSyntax);
        
        context.RegisterSourceOutput(classDeclarations, (spc, symbol) =>
        {
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debugger.Launch();
            }
            var source = GenerateSetterAndGetters(symbol!);
            spc.AddSource($"{symbol!.Name}.g.cs", SourceText.From(source, Encoding.UTF8));
        });
    }

    private static bool IsValidSyntax(INamedTypeSymbol? syntax)
    {
        return syntax is not null;
    }
    
    private static bool IsSyntaxTargetForGeneration(SyntaxNode node, CancellationToken token)
        => node is ClassDeclarationSyntax { AttributeLists.Count: > 0 };

    private static INamedTypeSymbol? GetSemanticTargetForGeneration(GeneratorSyntaxContext context, CancellationToken token)
    {
        var classSyntax = (ClassDeclarationSyntax)context.Node;
        foreach (var attributeListSyntax in classSyntax.AttributeLists)
        {
            foreach (var attributeSyntax in attributeListSyntax.Attributes)
            {
                var attributeSymbol = context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol;
                if (attributeSymbol is null)
                {
                    continue;
                }
                
                if (attributeSymbol.ContainingType.ToDisplayString() == "Base.GeneratedPropertyCollectionAttribute")
                {
                    return context.SemanticModel.GetDeclaredSymbol(classSyntax) as INamedTypeSymbol;
                }
            }
        }
        
        return null;
    }

    private string GenerateSetterAndGetters(INamedTypeSymbol symbol)
    {
        List<IPropertySymbol> properties = new();
        foreach (var prop in symbol.GetMembers().OfType<IPropertySymbol>())
        {
            if(prop is null) continue;
            
            var ok = false;
            foreach (var attr in prop.GetAttributes())
            {
                if (attr.AttributeClass?.ToDisplayString() == "Base.GeneratedPropertyCollection")
                {
                    ok = true;
                    break;
                }
            }

            if(ok) properties.Add(prop);
        }
        
        
        var result = new StringBuilder(GetClassStart(symbol));

        foreach (var prop in properties)
        {
            result.Append(GetPropertyGetterAndSetter(prop));
        }

        result.Append(GetClassEnd(properties));

        return result.ToString();
    }

    private static string GetClassStart(INamedTypeSymbol symbol)
    {
        return $$"""
                 namespace {{symbol.ContainingNamespace.ToDisplayString()}};

                 public partial class {{symbol.ContainingType.Name}}
                 {
                 """;
    }

    private static string GetPropertyGetterAndSetter(IPropertySymbol symbol)
    {
        var name = symbol.ToDisplayString();
        var privateName = "_" + name.ToLower();
        
        return $$"""
                     private {{symbol.Type.ToDisplayString()}} {{privateName}};

                     public {{symbol.Type.ToDisplayString()}} {{name}}
                     {
                         get 
                         {
                             BeforeGetValue(name);
                             var val = {{privateName}};
                             AfterGetValue(name, val);
                             return val;
                         }
                         set
                         {
                             BeforeSetValue(name, value);
                             {{privateName}} = value;
                             AfterSetValue(name, value);
                         }
                     }
                     
                 """;
    }
    
    private static string GetClassEnd(IReadOnlyList<IPropertySymbol> properties)
    {
        return $$"""
               public override int GetPropertyCount() {
                    return {{properties.Count}};
               }
               
               public override IEnumerable<string> GetPropertiesName() {
                    return [{{string.Join(", ", properties.Select(p => p.ToDisplayString()))}}];
               }
               
               }
               """;
    }
}