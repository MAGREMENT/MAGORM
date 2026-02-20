using Core.Abstract;

namespace Core.FieldDefinitions.ValueTypes;

public class BoolValueFieldDefinition(string name, FieldDefinitionsOptions options) 
    : ValueFieldDefinition<bool>(name, options)
{
    public override DBFieldType GetFieldType() => DBFieldType.BOOL;

    protected override bool TryConvert(object value, out bool result)
    {
        if (value is bool b)
        {
            result = b;
            return true;
        }

        if (value is int i)
        {
            result = i != 0;
            return true;
        }

        result = false;
        return false;
    }
}