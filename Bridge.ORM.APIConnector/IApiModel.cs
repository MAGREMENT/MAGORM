using APIConnector;
using ORM.Abstract;
using ORM.RecordTypes;

namespace Bridge.ORM.APIConnector;

public interface IApiModel : IModel
{
    public void DefineEndpoints(IEndpointDefiner definer);
}

[Flags]
public enum DefaultEndpointOperations
{
    CREATE = 0b1,
    READ = 0b10,
    UPDATE = 0b100,
    DELETE = 0b1000,
    
    ALL = CREATE | READ | UPDATE | DELETE
}

public static class EndpointModelExtensions
{
    public static void DefineDefaultEndpoints(this IModel model, IEndpointDefiner definer, DefaultEndpointOperations operations)
    {
        if((operations & DefaultEndpointOperations.CREATE) != 0)
            definer.Define(GetCreateSpec(model));
    }

    private static EndpointSpecification<RecordDictionary[]> GetCreateSpec(IModel model)
    {
        return new EndpointSpecification<RecordDictionary[]>(EndpointType.POST, "/" + model.Name.ToLower() + "/create", 
            (records) => model.Create<DictionarySideEffectRecord>(records));
    }
}