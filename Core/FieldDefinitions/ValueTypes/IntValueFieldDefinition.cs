using Core.Abstract;

namespace Core.FieldDefinitions.ValueTypes;

public class IntValueFieldDefinition(string name, FieldDefinitionsOptions options) 
    : ValueFieldDefinition(name, options)
{
    public override DBFieldType GetDBFieldType() => DBFieldType.INT;
    
    public override bool TryConvert(object value, out object? result)
    {
        switch (value)
        {
            case long l : 
                result = (int)l;
                return true;
            case int i : 
                result = i;
                return true;
            default:
                result = null;
                return false;
        }
    }
}