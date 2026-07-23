using Microsoft.CodeAnalysis;

namespace BaseGenerator;

[Generator]
public class Generator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        PropertyFieldCollectionGenerator.Initialize(context);
        ExtensibleGenerator.Initialize(context);
    }
}