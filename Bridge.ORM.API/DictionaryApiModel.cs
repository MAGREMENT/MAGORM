using API;
using ORM.Abstract;
using ORM.ModelTypes;

namespace Bridge.ORM.API;

public class DictionaryApiModel(string name, IFieldDefinition primaryKey, params IFieldDefinition[] fields)
    : DictionaryModel(name, primaryKey, fields), IEndpointProvider
{
    public void DefineEndpoints(IEndpointDefiner definer)
    {
        this.DefineDefaultEndpoints(definer, [
            new DefaultEndpointDefinition(DefaultEndpointOperations.CREATE, DefaultEndpointParameters())
        ]);
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