using Core.Abstract;
using Core.FieldDefinitions.ValueTypes;

namespace Core;

public static class Fields
{
    public static IFieldDefinition Int(string name, FieldDefinitionsOptions options)
        => new IntValueFieldDefinition(name, options);
    
    public static IFieldDefinition String(string name, FieldDefinitionsOptions options)
        => new StringValueFieldDefinition(name, options);
    
    public static IFieldDefinition Bool(string name, FieldDefinitionsOptions options)
        => new BoolValueFieldDefinition(name, options);
}