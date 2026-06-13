using ORM.Abstract;
using ORM.ModelTypes;

namespace ORM;

public static class Models
{
    public static readonly IFieldDefinition BasePrimaryKey = Fields.Int("Id", new FieldDefinitionsOptions
    {
        AutoIncrement = true,
        Required = true,
        Unique = true
    });

    public static Model DefineBase(string name, params IFieldDefinition[] fields)
        => new DictionaryModel(name, BasePrimaryKey, fields);
}