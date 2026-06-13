using System;
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
    
    public static void CreateMethodSignature(StringBuilder builder, IMethodSymbol symbol, Action<StringBuilder>? additionalParameters = null)
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
}