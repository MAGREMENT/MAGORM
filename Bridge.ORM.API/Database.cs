using API;
using ORM.Abstract;

namespace Bridge.ORM.API;

public static class DatabaseExtensions
{
    public static void AddModelsEndpoints(this Database database, IEndpointDefiner definer)
    {
        foreach (var model in database.EnumerateModels())
        {
            if(model is not IApiModel em) continue;
            em.DefineEndpoints(definer);
        }
    }
}