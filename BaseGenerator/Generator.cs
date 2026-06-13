using Microsoft.CodeAnalysis;

namespace BaseGenerator;

[Generator]
public class Generator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        PropertyCollectionGenerator.Initialize(context);
        ExtensionTemplateGenerator.Initialize(context);
    }

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
}