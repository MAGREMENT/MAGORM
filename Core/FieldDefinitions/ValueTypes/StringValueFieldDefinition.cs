using Core.Abstract;

namespace Core.FieldDefinitions.ValueTypes;

public class StringValueFieldDefinition(string name, FieldDefinitionsOptions options) 
    : ValueFieldDefinition(name, options)
{
    public override DBFieldType GetDBFieldType() => DBFieldType.STRING;
}