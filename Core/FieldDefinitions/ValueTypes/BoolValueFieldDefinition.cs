using Core.Abstract;

namespace Core.FieldDefinitions.ValueTypes;

public class BoolValueFieldDefinition(string name, FieldDefinitionsOptions options) 
    : ValueFieldDefinition(name, options)
{
    public override DBFieldType GetDBFieldType() => DBFieldType.BOOL;
    
    public override bool TryConvert(object value, out object? result)
    {
        switch (value)
        {
            case long l : return TryConvertInt((int)l, out result);
            case int i : return TryConvertInt(i, out result);
            case bool b :
                result = b;
                return true;
            default:
                result = null;
                return false;
        }
    }

    private bool TryConvertInt(int value, out object? result)
    {
        result = value != 0;
        return true;
    }
}