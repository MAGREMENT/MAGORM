using Core.Abstract;

namespace Core.FieldDefinitions.ValueTypes;

public class BoolValueFieldDefinition(string name, FieldDefinitionsOptions options) 
    : ValueFieldDefinition(name, options)
{
    public override DBFieldType GetDBFieldType() => DBFieldType.BOOL;
}