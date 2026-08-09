using Microsoft.CodeAnalysis;

namespace ORMGenerator;

[Generator]
public class Generator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        ModelDefinitionGenerator.Initialize(context);
    }
}