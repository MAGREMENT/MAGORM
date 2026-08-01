using API;
using ORM.Abstract;

namespace Bridge.ORM.API;

public static class DatabaseExtensions
{
    public static void DefineModelsEndpoints(this Database database, IEndpointDefiner definer)
    {
        foreach (var model in database.EnumerateModels())
        {
            if(model is not IEndpointProvider ep) continue;
            ep.DefineEndpoints(definer);
        }
    }
}