using System.Diagnostics.CodeAnalysis;
using Core.Abstract;

namespace Core.FieldDefinitions;

public class ComputedFieldDefinition<TType>(string name, TryCompute<TType, IRecord> tryCompute, 
    IReadOnlyList<string> dependsOn, FieldDefinitionsOptions options) : IFieldDefinition
{
    public string Name { get; } = name;

    public FieldDefinitionsOptions Options { get; } = options with { AutoIncrement = false};

    public DBFieldType GetDBFieldType() => DBFieldTypeExtensions.ToDBFieldType<TType>();

    public bool Stored { get; set; }

    public bool IsStored() => Stored;

    public IReadOnlyList<string> DependsOn { get; } = dependsOn;
    
    public ModelReference[] References => [];

    public bool TryComputeValue<T>(object? value, T record, [NotNullWhen(true)] out object? result) where T : IRecord
    {
        if (!tryCompute(record, out var r))
        {
            result = null;
            return false;
        }
        
        result = r!;
        return true;
    }

    public void AttachToDatabase(Database database) { }
}

public delegate bool TryCompute<TType, in TRecord>(TRecord record, out TType result) where TRecord : IRecord;