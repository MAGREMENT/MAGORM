using ORM.Abstract;
using ORM.Queries;

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

    public override bool TryFetchFromQueryResult(IQueryResult result, string name, out object? value)
    {
        if (_reference is null) throw new ArgumentException(); //TODO

        return _reference.Field.TryFetchFromQueryResult(result, name, out value);
    }

    public override object? ToDbValue(object? recordValue)
    {
        if (_reference is null) throw new ArgumentException(); //TODO
        if (recordValue is not IRecord otherRecord) throw new ArgumentException(); //TODO
        
        return otherRecord.Get(_reference.Field.Name);
    }

    public override void Attach(IReadOnlyModelRegistry obj)
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

    public override void Detach(IReadOnlyModelRegistry obj)
    {
        _reference = null;
    }
}