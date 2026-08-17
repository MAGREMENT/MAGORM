using ORM.Abstract;

namespace ORM.FieldDefinitions;

public abstract class ValueFieldDefinition(string name, FieldDefinitionsOptions options) : FieldDefinition
{
    public override string Name { get; } = name;
    public override FieldDefinitionsOptions Options { get; } = options;
    public abstract override DBFieldType GetDBFieldType();
    public override ModelReference? Reference => null;

    public override bool TryComputeValue<T>(object? value, T record, out object? result)
    {
        if (value is null)
        {
            result = null;
            return !Options.Required;
        }

        return TryConvert(value, out result); //TODO make a fonction that call queryResult.GetInt() instead maybe ?
    }

    public abstract bool TryConvert(object value, out object? result);

    public override object? ToDbValue(object? recordValue) => recordValue;

    public override void Attach(IReadOnlyModelBank obj) { }

    public override void Detach(IReadOnlyModelBank obj) { }
}