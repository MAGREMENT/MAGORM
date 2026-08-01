using ORM.Abstract;
using WebServer;

namespace Bridge.ORM.WebServer;

public static class DatabaseExtensions
{
    public static void DefineModelsViews(this Database database, IViewDefiner definer)
    {
        foreach (var model in database.EnumerateModels())
        {
            if(model is not IViewProvider ep) continue;
            ep.DefineViews(definer);
        }
    }
}