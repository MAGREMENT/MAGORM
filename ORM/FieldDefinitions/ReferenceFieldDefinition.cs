using ORM.Abstract;

namespace ORM.FieldDefinitions;

public class ReferenceFieldDefinition(string name, string _otherModelName, FieldDefinitionsOptions options, 
    string? otherModelField = null) 
    : FieldDefinition
{
    private IFieldDefinition? _referencedField;
    private ModelReference? _modelReference;

    public override string Name { get; } = name;
    public override FieldDefinitionsOptions Options { get; } = options with { AutoIncrement = false};
    public override ModelReference? References => _modelReference;

    public override DBFieldType GetDBFieldType()
    {
        if (_referencedField is null) throw new ArgumentException(); //TODO
        
        return _referencedField.GetDBFieldType();
    }

    public override bool TryComputeValue<T>(object? value, T record, out object? result)
    {
        if (_referencedField is null) throw new ArgumentException(); //TODO

        if (value is IRecord otherRecord) value = otherRecord.Get(_referencedField.Name);
        return _referencedField.TryComputeValue(value, record, out result);
    }

    public override void Attach(Database obj)
    {
        var otherModel = obj.GetModel(_otherModelName);
        if (otherModel is null) throw new ArgumentException(); //TODO

        var field = otherModelField is null
            ? otherModel.GetPrimaryKey()
            : otherModel.GetFieldDefinition(otherModelField);

        if (field is null) throw new ArgumentException(); //TODO

        _referencedField = field;
        _modelReference = new ModelReference(otherModel.Name, field.Name);
    }

    public override void Detach(Database obj)
    {
        _referencedField = null;
        _modelReference = null;
    }
}