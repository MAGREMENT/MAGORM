using Core.Abstract;

namespace Core.FieldDefinitions.ValueTypes;

public class StringValueFieldDefinition(string name, FieldDefinitionsOptions options) 
    : ValueFieldDefinition<string>(name, options)
{
    public override DBFieldType GetFieldType() => DBFieldType.VARCHAR;

    protected override bool TryConvert(string value, out string result)
    {
        result = value;
        return true;
    }
}