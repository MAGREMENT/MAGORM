using System.Diagnostics.CodeAnalysis;
using Core.Abstract;

namespace Core.FieldDefinitions;

public class ReferenceFieldDefinition<TModel>(string name, FieldDefinitionsOptions options, string? otherModelField = null) 
    : IFieldDefinition
    where TModel : Model
{
    private DBFieldType? _referenceFieldType;
    
    public string Name { get; } = name;
    public IReadOnlyList<string> DependsOn => [];
    public FieldDefinitionsOptions Options { get; } = options with { AutoIncrement = false};

    public DBFieldType GetDBFieldType()
    {
        if (_referenceFieldType is null) throw new ArgumentException(); //TODO

        return _referenceFieldType.Value;
    }

    public bool IsStored() => true;

    public ModelReference[] References => [];

    public bool TryComputeValue<T>(object? value, T record, [NotNullWhen(true)] out object? result) where T : IRecord
    {
        result = value!;
        return true; //TODO attach converter to DBFieldType and use it here
    }

    public void AttachToDatabase(Database database)
    {
        var otherModel = database.GetModel<TModel>();
        if (otherModel is null) throw new ArgumentException(); //TODO

        var field = otherModelField is null
            ? otherModel.GetPrimaryKey()
            : otherModel.GetFieldDefinition(otherModelField);

        if (field is null) throw new ArgumentException(); //TODO

        _referenceFieldType = field.GetDBFieldType();
    }
}