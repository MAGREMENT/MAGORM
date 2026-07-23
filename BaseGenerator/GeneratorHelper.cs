using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace BaseGenerator;

public static class GeneratorHelper
{
    public static bool IsValidSyntax(object? syntax)
    {
        return syntax is not null;
    }
    
    public static bool IsOrInherits(INamedTypeSymbol? symbol, string type)
    {
        while (symbol is not null)
        {
            if (symbol.ToDisplayString() == type) return true;
            symbol = symbol.BaseType;
        }

        return false;
    }
    
    public static void CreateMethodSignature(StringBuilder builder, IMethodSymbol symbol, 
        Action<StringBuilder>? additionalParameters = null, params string[] methodQualifier)
    {
        builder.Append("\tpublic ");
        foreach (var q in methodQualifier)
        {
            builder.Append(q);
            builder.Append(' ');
        }
        builder.Append(symbol.ReturnType.ToDisplayString());
        builder.Append(' ');
        builder.Append(symbol.Name);
        builder.Append('(');
        for (int i = 0; i < symbol.Parameters.Length; i++)
        {
            if (i != 0) builder.Append(", ");
            builder.Append(symbol.Parameters[i].Type.ToDisplayString());
            builder.Append(' ');
            builder.Append(symbol.Parameters[i].Name);
        }
        if(additionalParameters is not null) additionalParameters(builder);

        builder.Append(") {\n");
    }

    public static List<IPropertySymbol> GetAllPropertiesWithAttribute(INamedTypeSymbol symbol, string attributeFullName)
    {
        List<IPropertySymbol> properties = new();
        foreach (var prop in symbol.GetMembers().OfType<IPropertySymbol>())
        {
            var ok = false;
            foreach (var attr in prop.GetAttributes())
            {
                if (IsOrInherits(attr.AttributeClass, attributeFullName))
                {
                    ok = true;
                    break;
                }
            }

            if(ok) properties.Add(prop);
        }

        return properties;
    }
}