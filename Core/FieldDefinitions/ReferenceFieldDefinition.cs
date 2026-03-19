using Core.Abstract;

namespace Core.FieldDefinitions;

public class ReferenceFieldDefinition(string name, string _otherModelName, FieldDefinitionsOptions options, string? otherModelField = null) 
    : IFieldDefinition
{
    public IFieldDefinition? ReferenceField { get; private set; }

    public string Name { get; } = name;
    public IReadOnlyList<string> DependsOn => [];
    public FieldDefinitionsOptions Options { get; } = options with { AutoIncrement = false};

    public DBFieldType GetDBFieldType()
    {
        if (ReferenceField is null) throw new ArgumentException(); //TODO
        
        return ReferenceField.GetDBFieldType();
    }

    public bool IsStored() => true;

    public ModelReference[] References { get; private set; } = [];

    public bool TryComputeValue<T>(object? value, T record, out object? result) where T : IRecord
    {
        if (ReferenceField is null) throw new ArgumentException(); //TODO
        
        return ReferenceField.TryComputeValue(value, record, out result);
    }

    public void Attach(Model obj)
    {
        var otherModel = obj.Database?.GetModel(_otherModelName);
        if (otherModel is null) throw new ArgumentException(); //TODO

        var field = otherModelField is null
            ? otherModel.GetPrimaryKey()
            : otherModel.GetFieldDefinition(otherModelField);

        if (field is null) throw new ArgumentException(); //TODO

        ReferenceField = field;
        References = [new ModelReference(otherModel.Name, field.Name)];
    }

    public void Detach(Model obj)
    {
        ReferenceField = null;
        References = [];
    }
}