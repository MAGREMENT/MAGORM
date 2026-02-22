using Core.Abstract;

namespace Core.FieldDefinitions.ValueTypes;

public class IntValueFieldDefinition(string name, FieldDefinitionsOptions options) 
    : ValueFieldDefinition(name, options)
{
    public override DBFieldType GetDBFieldType() => DBFieldType.INT;
}