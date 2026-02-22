using Core.Abstract;
using Core.ModelTypes;

namespace Core;

public static class Models
{
    private static readonly IFieldDefinition _basePrimaryKey = Fields.Int("Id", new FieldDefinitionsOptions
    {
        AutoIncrement = true,
        Required = true,
        Unique = true
    });

    public static Model DefineBase(string name, params IFieldDefinition[] fields)
        => new DictionaryModel(name, _basePrimaryKey, fields);
}