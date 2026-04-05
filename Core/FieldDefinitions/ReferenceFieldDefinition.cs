using Core.Abstract;

namespace Core.FieldDefinitions;

public class ReferenceFieldDefinition(string name, string _otherModelName, FieldDefinitionsOptions options, 
    string? otherModelField = null) 
    : IFieldDefinition
{
    private IFieldDefinition? _referenceField;

    public string Name { get; } = name;
    public FieldDefinitionsOptions Options { get; } = options with { AutoIncrement = false};
    public ModelReference[] References { get; private set; } = [];

    public DBFieldType GetDBFieldType()
    {
        if (_referenceField is null) throw new ArgumentException(); //TODO
        
        return _referenceField.GetDBFieldType();
    }

    public bool TryComputeValue<T>(object? value, T record, out object? result) where T : IRecord
    {
        if (_referenceField is null) throw new ArgumentException(); //TODO
        
        return _referenceField.TryComputeValue(value, record, out result);
    }

    public void Attach(Model obj)
    {
        var otherModel = obj.Database?.GetModel(_otherModelName);
        if (otherModel is null) throw new ArgumentException(); //TODO

        var field = otherModelField is null
            ? otherModel.GetPrimaryKey()
            : otherModel.GetFieldDefinition(otherModelField);

        if (field is null) throw new ArgumentException(); //TODO

        _referenceField = field;
        References = [new ModelReference(otherModel.Name, field.Name)];
    }

    public void Detach(Model obj)
    {
        _referenceField = null;
        References = [];
    }
}