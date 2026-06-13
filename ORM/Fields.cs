using ORM.Abstract;
using ORM.FieldDefinitions;
using ORM.FieldDefinitions.ValueTypes;

namespace ORM;

public static class Fields
{
    public static IFieldDefinition Int(string name, FieldDefinitionsOptions? options = null)
        => new IntValueFieldDefinition(name, options ?? new FieldDefinitionsOptions());
    
    public static IFieldDefinition String(string name, FieldDefinitionsOptions? options = null)
        => new StringValueFieldDefinition(name, options ?? new FieldDefinitionsOptions());
    
    public static IFieldDefinition Bool(string name, FieldDefinitionsOptions? options = null)
        => new BoolValueFieldDefinition(name, options ?? new FieldDefinitionsOptions());

    public static IFieldDefinition Reference(string name, string otherModelName, string? otherFieldName = null, 
        FieldDefinitionsOptions? options = null)
        => new ReferenceFieldDefinition(name, otherModelName, options ?? new FieldDefinitionsOptions(), otherFieldName);
}