using ORM.Abstract;

namespace ORM.FieldDefinitions;

public class ReferenceFieldDefinition(string name, string _otherModelName, FieldDefinitionsOptions options, 
    string? otherModelField = null) 
    : FieldDefinition
{
    private ModelReference? _reference;

    public override string Name { get; } = name;
    public override FieldDefinitionsOptions Options { get; } = options with { AutoIncrement = false};
    public override ModelReference? Reference => _reference;

    public override DBFieldType GetDBFieldType()
    {
        if (_reference is null) throw new ArgumentException(); //TODO
        
        return _reference.Field.GetDBFieldType();
    }

    public override bool TryComputeValue<T>(object? value, T record, out object? result)
    {
        if (_reference is null) throw new ArgumentException(); //TODO

        if (value is IRecord otherRecord) value = otherRecord.Get(_reference.Field.Name);
        return _reference.Field.TryComputeValue(value, record, out result);
    }

    public override object? ToDbValue(object? recordValue)
    {
        if (_reference is null) throw new ArgumentException(); //TODO
        if (recordValue is not IRecord otherRecord) throw new ArgumentException(); //TODO
        
        return otherRecord.Get(_reference.Field.Name);
    }

    public override void Attach(IReadOnlyModelBank obj)
    {
        if (_reference is not null) throw new Exception("Field already attached");
        
        var otherModel = obj.GetModel(_otherModelName);
        if (otherModel is null) throw new ArgumentException(); //TODO

        var field = otherModelField is null
            ? otherModel.GetPrimaryKey()
            : otherModel.GetFieldDefinition(otherModelField);

        if (field is null) throw new ArgumentException(); //TODO
        
        _reference = new ModelReference(otherModel, field);
    }

    public override void Detach(IReadOnlyModelBank obj)
    {
        _reference = null;
    }
}