using API;
using ORM.Abstract;
using ORM.ModelTypes;

namespace Bridge.ORM.API;

public class DictionaryApiModel(string name, IFieldDefinition primaryKey, params IFieldDefinition[] fields)
    : DictionaryModel(name, primaryKey, fields), IEndpointProvider
{
    private readonly List<Endpoint> _endpoints = new();

    public void AddEndpoint(Endpoint endpoint) => _endpoints.Add(endpoint);
    
    public void DefineEndpoints(IEndpointDefiner definer)
    {
        this.DefineDefaultEndpoints(definer, [
            new DefaultEndpointDefinition(DefaultEndpointOperations.CREATE, DefaultEndpointParameters())
        ]);

        foreach (var endpoint in _endpoints)
        {
            definer.DefineEndpoint(endpoint);
        }
    }

    private IEnumerable<EndpointParameterCheck> DefaultEndpointParameters()
    {
        foreach (var val in AllFieldDefinitions)
        {
            if(val.Options.AutoIncrement) continue;
            yield return new EndpointParameterCheck(val.Name, typeof(string) /*TODO*/);
        }
    }
}