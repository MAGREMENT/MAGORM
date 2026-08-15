using API;
using ORM.Abstract;
using ORM.RecordTypes;

namespace Bridge.ORM.API;

public static class ApiModelGenerator
{
    public static IModel Generate(Type type)
    {
        var (primary, definitions) = ModelField.GetFields(type);
        if (primary is null) throw new Exception("Need primary key");
        
        var result = new DictionaryApiModel(type.Name, primary, definitions);
        var endpoints = EndpointMethod.GetEndpoints(type);
        result.AddEndpoints(endpoints);
        
        return result;
    }
}