using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace BaseGenerator;

public static class ExtensibilityGeneratorHelper
{
    public static Dictionary<string, IMethodSymbol> GetBaseMethods(INamedTypeSymbol symbol)
    {
        Dictionary<string, IMethodSymbol> baseMethods = new();
        foreach (var method in symbol.GetMembers().OfType<IMethodSymbol>())
        {
            AttributeData? wantedAttribute = null;
            foreach (var a in method.GetAttributes())
            {
                if (a.AttributeClass?.Name == "ExtensionBaseMethodAttribute")
                {
                    wantedAttribute = a;
                }
            }
            if(wantedAttribute is null) continue;

            baseMethods[(string)wantedAttribute.ConstructorArguments[0].Value!] = method;
        }

        return baseMethods;
    }
}