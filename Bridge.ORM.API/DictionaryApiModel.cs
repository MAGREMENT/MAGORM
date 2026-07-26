using API;
using ORM.Abstract;
using ORM.ModelTypes;

namespace Bridge.ORM.API;

public class DictionaryApiModel(string name, IFieldDefinition primaryKey, IFieldDefinition[] fields)
    : DictionaryModel(name, primaryKey, fields), IApiModel
{
    public void DefineEndpoints(IEndpointDefiner definer)
    {
        this.DefineDefaultEndpoints(definer, [
            new DefaultEndpointDefinition(DefaultEndpointOperations.CREATE, DefaultEndpointParameters())
        ]);
    }

    private IEnumerable<EndpointParameter> DefaultEndpointParameters()
    {
        foreach (var val in AllFieldDefinitions)
        {
            yield return new EndpointParameter(val.Name, typeof(string) /*TODO*/);
        }
    }
}