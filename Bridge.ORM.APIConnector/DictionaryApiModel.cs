using APIConnector;
using ORM.Abstract;
using ORM.ModelTypes;

namespace Bridge.ORM.APIConnector;

public class DictionaryApiModel(string name, IFieldDefinition primaryKey, IFieldDefinition[] fields)
    : DictionaryModel(name, primaryKey, fields), IApiModel
{
    public void DefineEndpoints(IEndpointDefiner definer)
    {
        this.DefineDefaultEndpoints(definer, DefaultEndpointOperations.ALL);
    }
}
    
    