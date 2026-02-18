using Core.Abstract;

namespace Core.FieldDefinitions.ValueTypes;

public class IntValueFieldDefinition(string name, FieldDefinitionsOptions options) 
    : ValueFieldDefinition<int>(name, options)
{
    public override DBFieldType GetFieldType() => DBFieldType.INT;

    protected override bool TryConvert(string value, out int result)
    {
        return int.TryParse(value, out result);
    }
}