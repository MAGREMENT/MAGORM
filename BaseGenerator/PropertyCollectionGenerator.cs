using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace BaseGenerator;

public static class PropertyCollectionGenerator
{
    public static void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var declarations = context.SyntaxProvider
            .CreateSyntaxProvider(IsSyntaxTargetForGeneration,
                GetSemanticTargetForGeneration)
            .Where(GeneratorHelper.IsValidSyntax);
        
        context.RegisterSourceOutput(declarations, (spc, symbol) =>
        {
            var source = GenerateSetterAndGetters(symbol!);
            spc.AddSource($"{symbol!.Name}.g.cs", SourceText.From(source, Encoding.UTF8));
        });
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
                
                if (GeneratorHelper.IsOrInherits(attributeSymbol?.ContainingType, "Base.Fields.SideEffectFieldCollectionAttribute"))
                {
                    return ModelExtensions.GetDeclaredSymbol(context.SemanticModel, classSyntax) as INamedTypeSymbol;
                }
            }
        }
        
        return null;
    }
    
    private static string GenerateSetterAndGetters(INamedTypeSymbol symbol)
    {
        List<IPropertySymbol> properties = new();
        foreach (var prop in symbol.GetMembers().OfType<IPropertySymbol>())
        {
            var ok = false;
            foreach (var attr in prop.GetAttributes())
            {
                if (GeneratorHelper.IsOrInherits(attr.AttributeClass, "Base.Fields.SideEffectFieldAttribute"))
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

        //TODO if (IsOrInherits(symbol.ContainingType, "Base.Fields.Implementations.ClassSideEffectFieldCollection"))
            result.Append(GetClassEnd(properties));

        return result.ToString();
    }

    private static string GetClassStart(INamedTypeSymbol symbol)
    {
        return $$"""
                 namespace {{symbol.ContainingNamespace.ToDisplayString()}};

                 public partial class {{symbol.Name}}
                 {
                 """;
    }

    private static string GetPropertyGetterAndSetter(IPropertySymbol symbol)
    {
        var name = symbol.Name;
        var stringifiedName = $"\"{name}\"";
        var privateName = "_" + name.ToLower();
        var type = symbol.Type.ToDisplayString();
        
        return $$"""
                 
                     private {{type}} {{privateName}};

                     public partial {{type}} {{name}}
                     {
                         get 
                         {
                             return ({{type}})_sideEffects.NextGet(this, _sideEffects.BaseState, {{stringifiedName}});
                         }
                         set
                         {
                             _sideEffects.NextSet(this, _sideEffects.BaseState, {{stringifiedName}}, value);
                         }
                     }
                     
                 """;
    }
    
    private static string GetClassEnd(IReadOnlyList<IPropertySymbol> properties)
    {
        var setBuilder = new StringBuilder();
        var getBuilder = new StringBuilder();

        foreach (var prop in properties)
        {
            setBuilder.Append("\t\tcase \"");
            setBuilder.Append(prop.Name);
            setBuilder.Append("\" : _");
            setBuilder.Append(prop.Name.ToLower());
            setBuilder.Append(" = (");
            setBuilder.Append(prop.Type.ToDisplayString());
            setBuilder.Append(")value; break;");

            getBuilder.Append("\t\t\"");
            getBuilder.Append(prop.Name);
            getBuilder.Append("\" => _");
            getBuilder.Append(prop.Name.ToLower());
        }
        
        return $$"""
               
                   public override int GetFieldCount() {
                        return {{properties.Count}};
                   }
                   
                   public override IEnumerable<string> GetFieldsName() {
                        return [{{string.Join(", ", properties.Select(p => $"\"{p.Name}\""))}}];
                   }
                   
                   protected override void InternalSetValue(string name, object? value)
                   {
                       switch(name) {
                           {{setBuilder}}
                       }
                   }
                   
                   protected override object? InternalGetValue(string name)
                   {
                       return name switch {
                            {{getBuilder}}
                       };
                   }
               }
               """;
    }
}