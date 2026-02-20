using Core.Abstract;

namespace Core.FieldDefinitions.ValueTypes;

public class IntValueFieldDefinition(string name, FieldDefinitionsOptions options) 
    : ValueFieldDefinition<int>(name, options)
{
    public override DBFieldType GetFieldType() => DBFieldType.INT;

    protected override bool TryConvert(object value, out int result)
    {
        if (value is int i)
        {
            result = i;
            return true;
        }

        result = 0;
        return false;
    }
}