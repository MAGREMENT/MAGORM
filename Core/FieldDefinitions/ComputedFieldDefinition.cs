using System.Diagnostics.CodeAnalysis;
using Core.Abstract;

namespace Core.FieldDefinitions;

public class ComputedFieldDefinition<TType>(string name, TryCompute<TType, IRecord> tryCompute, 
    IReadOnlyList<string> dependsOn, FieldDefinitionsOptions options) : IFieldDefinition
{
    private readonly TryCompute<TType, IRecord> _tryCompute = tryCompute;
    
    public string Name { get; } = name;

    public FieldDefinitionsOptions Options { get; } = options;

    public DBFieldType GetFieldType() => DBFieldTypeExtensions.ToDBFieldType<TType>();

    public bool Stored { get; set; }

    public bool IsStored() => Stored;

    public IReadOnlyList<string> DependsOn { get; } = dependsOn;

    public bool TryFetchValue<T>(string? value, T record, [NotNullWhen(true)] out object? result) where T : IRecord
    {
        if (!_tryCompute(record, out var r))
        {
            result = null;
            return false;
        }
        
        result = r!;
        return true;
    }
}

public delegate bool TryCompute<TType, in TRecord>(TRecord record, out TType result) where TRecord : IRecord;