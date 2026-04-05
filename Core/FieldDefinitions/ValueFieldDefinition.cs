using Core.Abstract;

namespace Core.FieldDefinitions;

public abstract class ValueFieldDefinition(string name, FieldDefinitionsOptions options) : IFieldDefinition
{
    public string Name { get; } = name;
    public FieldDefinitionsOptions Options { get; } = options;
    public abstract DBFieldType GetDBFieldType();
    public ModelReference[] References => [];

    public bool TryComputeValue<T>(object? value, T record, out object? result) where T : IRecord
    {
        if (value is null)
        {
            result = null;
            return !Options.Required;
        }

        return TryConvert(value, out result); //TODO make a fonction that call queryResult.GetInt() instead maybe ?
    }

    public abstract bool TryConvert(object value, out object? result);
    
    public void Attach(Model obj)
    {
    }

    public void Detach(Model obj)
    {
    }
}