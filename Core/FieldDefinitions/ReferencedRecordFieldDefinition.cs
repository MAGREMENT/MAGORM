using Core.Abstract;

namespace Core.FieldDefinitions;

public class ReferencedRecordFieldDefinition(string name, string referenceFieldName, FieldDefinitionsOptions options) 
    : IFieldDefinition
{
    private ReferenceFieldDefinition? _referenceField;
    
    public string Name { get; } = name;

    public IReadOnlyList<string> DependsOn { get; } = [referenceFieldName];

    public FieldDefinitionsOptions Options { get; } = options;

    public DBFieldType GetDBFieldType() => DBFieldType.NOT_DB_FIELD;

    public bool IsStored() => false;

    public ModelReference[] References { get; } = [];
    
    public bool TryComputeValue<T>(object? value, T record, out object? result) where T : IRecord
    {
        if (value is not IRecord r ||
            _referenceField is null ||
            _referenceField.ReferenceField is null ||
            !record.TryGetValue(_referenceField.Name, out var modelFieldValue) ||
            !r.TryGetValue(_referenceField.ReferenceField.Name, out var otherFieldValue) ||
            !modelFieldValue.Equals(otherFieldValue))
        {
            result = null;
            return false;
        }

        result = value;
        return true;
    }

    public void Attach(Model obj)
    {
        _referenceField = obj.GetFieldDefinition(DependsOn[0]) as ReferenceFieldDefinition;
        if (_referenceField is null) throw new Exception();
    }

    public void Detach(Model obj)
    {
        _referenceField = null;
    }
}