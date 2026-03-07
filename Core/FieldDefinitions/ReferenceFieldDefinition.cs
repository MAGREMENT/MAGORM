using System.Diagnostics.CodeAnalysis;
using Core.Abstract;

namespace Core.FieldDefinitions;

public class ReferenceFieldDefinition<TModel>(string name, FieldDefinitionsOptions options, string? otherModelField = null) 
    : IFieldDefinition
    where TModel : Model
{
    private IFieldDefinition? _referenceField;
    
    public string Name { get; } = name;
    public IReadOnlyList<string> DependsOn => [];
    public FieldDefinitionsOptions Options { get; } = options with { AutoIncrement = false};

    public DBFieldType GetDBFieldType()
    {
        if (_referenceField is null) throw new ArgumentException(); //TODO
        
        return _referenceField.GetDBFieldType();
    }

    public bool IsStored() => true;

    public ModelReference[] References => [];

    public bool TryComputeValue<T>(object? value, T record, out object? result) where T : IRecord
    {
        if (_referenceField is null) throw new ArgumentException(); //TODO
        
        return _referenceField.TryComputeValue(value, record, out result);
    }

    public void AttachToDatabase(Database database)
    {
        var otherModel = database.GetModel<TModel>();
        if (otherModel is null) throw new ArgumentException(); //TODO

        var field = otherModelField is null
            ? otherModel.GetPrimaryKey()
            : otherModel.GetFieldDefinition(otherModelField);

        if (field is null) throw new ArgumentException(); //TODO

        _referenceField = field;
    }
}