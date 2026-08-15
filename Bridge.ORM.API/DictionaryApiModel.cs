using API;
using ORM.Abstract;
using ORM.ModelTypes;

namespace Bridge.ORM.API;

public class DictionaryApiModel(string name, IFieldDefinition primaryKey, IEnumerable<IFieldDefinition> fields)
    : DictionaryModel(name, primaryKey, fields), IEndpointProvider
{
    private readonly List<Endpoint> _endpoints = new();

    public DictionaryApiModel(string name, IFieldDefinition primaryKey, params IFieldDefinition[] fields)
        : this(name, primaryKey, (IEnumerable<IFieldDefinition>)fields) {}

    public void AddEndpoint(Endpoint endpoint) => _endpoints.Add(endpoint);

    public void AddEndpoints(IEnumerable<Endpoint> endpoints) => _endpoints.AddRange(endpoints);
    
    public void DefineEndpoints(IEndpointDefiner definer)
    {
        this.DefineDefaultEndpoints(definer, [
            new DefaultEndpointDefinition(DefaultEndpointOperations.CREATE, [])
        ]);

        foreach (var endpoint in _endpoints)
        {
            definer.DefineEndpoint(endpoint);
        }
    }
}