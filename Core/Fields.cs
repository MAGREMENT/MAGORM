using Core.Abstract;
using Core.FieldDefinitions;
using Core.FieldDefinitions.ValueTypes;

namespace Core;

public static class Fields
{
    public static IFieldDefinition Int(string name, FieldDefinitionsOptions? options = null)
        => new IntValueFieldDefinition(name, options ?? new FieldDefinitionsOptions());
    
    public static IFieldDefinition String(string name, FieldDefinitionsOptions? options = null)
        => new StringValueFieldDefinition(name, options ?? new FieldDefinitionsOptions());
    
    public static IFieldDefinition Bool(string name, FieldDefinitionsOptions? options = null)
        => new BoolValueFieldDefinition(name, options ?? new FieldDefinitionsOptions());

    public static IFieldDefinition Computed<TType>(string name, TryCompute<TType, IRecord> tryCompute,
        FieldDefinitionsOptions? options = null, params string[] dependsOn) 
        => new ComputedFieldDefinition<TType>(name, tryCompute, dependsOn, options ?? new FieldDefinitionsOptions());

    public static IFieldDefinition Reference<TModel>(string name, FieldDefinitionsOptions? options = null,
        string? otherFieldName = null) where TModel : Model
        => new ReferenceFieldDefinition<TModel>(name, options ?? new FieldDefinitionsOptions(), otherFieldName);
}