using ORM;
using ORM.Abstract;

namespace Bridge.ORM.API;

public class ApiModels
{
    public static IApiModel DefineBase(string name, params IFieldDefinition[] fields)
        => new DictionaryApiModel(name, Models.BasePrimaryKey, fields);
}