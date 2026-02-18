using Core.Abstract;

namespace Core.FieldDefinitions.ValueTypes;

public class BoolValueFieldDefinition(string name, FieldDefinitionsOptions options) 
    : ValueFieldDefinition<bool>(name, options)
{
    public override DBFieldType GetFieldType() => DBFieldType.BOOL;

    protected override bool TryConvert(string value, out bool result)
    {
        switch (value)
        {
            case "true":
            case "True":
                result = true;
                return true;
            case "false":
            case "False":
                result = false;
                return true;
            default:
                if (!int.TryParse(value, out var i))
                {
                    result = false;
                    return false;
                }

                result = i != 0;
                return true;
        }
    }
}