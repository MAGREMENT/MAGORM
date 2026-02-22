using System.Diagnostics.CodeAnalysis;
using Core.Abstract;

namespace Core.FieldDefinitions;

public abstract class ValueFieldDefinition(string name, FieldDefinitionsOptions options) : IFieldDefinition
{
    public string Name { get; } = name;

    public FieldDefinitionsOptions Options { get; } = options;

    public abstract DBFieldType GetDBFieldType();

    public bool IsStored() => true;

    public IReadOnlyList<string> DependsOn => [];
    
    public ModelReference[] References => [];

    public bool TryComputeValue<T>(object? value, T record, [NotNullWhen(true)] out object? result) where T : IRecord
    {
        result = value!;
        return true;
    }
    
    public void AttachToDatabase(Database database) { }
}