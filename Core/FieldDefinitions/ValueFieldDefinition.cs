using System.Diagnostics.CodeAnalysis;
using Core.Abstract;

namespace Core.FieldDefinitions;

public abstract class ValueFieldDefinition<TType>(string name, FieldDefinitionsOptions options) : IFieldDefinition
{
    public string Name { get; } = name;

    public FieldDefinitionsOptions Options { get; } = options;

    public abstract DBFieldType GetDBFieldType();

    public bool IsStored() => true;

    public IReadOnlyList<string> DependsOn => [];
    
    public ModelReference[] References => [];

    public bool TryComputeValue<T>(object? value, T record, [NotNullWhen(true)] out object? result) where T : IRecord
    {
        if (value is null || !TryConvert(value, out var r))
        {
            result = null;
            return false;
        }

        result = r!;
        return true;
    }

    protected abstract bool TryConvert(object value, out TType result);
    
    public void AttachToDatabase(Database database) { }
}