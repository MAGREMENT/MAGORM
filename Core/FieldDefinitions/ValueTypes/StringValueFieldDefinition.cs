using Core.Abstract;

namespace Core.FieldDefinitions.ValueTypes;

public class StringValueFieldDefinition(string name, FieldDefinitionsOptions options) 
    : ValueFieldDefinition<string>(name, options)
{
    public override DBFieldType GetFieldType() => DBFieldType.VARCHAR;

    protected override bool TryConvert(object value, out string result)
    {
        if (value is string i)
        {
            result = i;
            return true;
        }

        result = string.Empty;
        return false;
    }
}