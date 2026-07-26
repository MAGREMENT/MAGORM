using API;
using ORM.Abstract;
using ORM.RecordTypes;

namespace Bridge.ORM.API;

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

public record DefaultEndpointDefinition(DefaultEndpointOperations Operation, IEnumerable<EndpointParameter> Parameters);

public static class EndpointModelExtensions
{
    public static void DefineDefaultEndpoints(this IModel model, IEndpointDefiner definer,
        DefaultEndpointDefinition[] definitions)
    {
        foreach (var definition in definitions)
        {
            switch (definition.Operation)
            {
                case DefaultEndpointOperations.CREATE:
                    definer.Define(new Endpoint(EndpointType.POST, "/" + model.Name.ToLower() + "/create"), 
                        definition.Parameters, model.Create<DictionaryRecord>);
                    break;
            }
        }
    }
}