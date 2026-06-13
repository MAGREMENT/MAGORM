using ORM.Abstract;

namespace ORM.FieldDefinitions.ValueTypes;

public class StringValueFieldDefinition(string name, FieldDefinitionsOptions options) 
    : ValueFieldDefinition(name, options)
{
    public override DBFieldType GetDBFieldType() => DBFieldType.STRING;
    
    public override bool TryConvert(object value, out object? result)
    {
        switch (value)
        {
            case string s :
                result = s;
                return true;
            default:
                result = null;
                return false;
        }
    }
}