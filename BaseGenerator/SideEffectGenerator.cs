using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace BaseGenerator;

public static class SideEffectGenerator //TODO
{
    private const string SideEffectAttributeFullName = "Base.SideEffect.SideEffectAttribute";
    
    private static string GenerateSetterAndGetters(INamedTypeSymbol symbol)
    {
        List<IPropertySymbol> properties = new();
        foreach (var prop in symbol.GetMembers().OfType<IPropertySymbol>())
        {
            var ok = false;
            foreach (var attr in prop.GetAttributes())
            {
                if (GeneratorHelper.IsOrInherits(attr.AttributeClass, SideEffectAttributeFullName))
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
}