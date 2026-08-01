using Bridge.ORM.API;
using ORM.Abstract;
using WebServer;

namespace Bridge.ORM.WebServer;

public class DictionaryViewApiModel(string name, IFieldDefinition primaryKey, params IFieldDefinition[] fields) 
    : DictionaryApiModel(name, primaryKey, fields), IViewProvider
{
    private readonly List<View> _views = new();

    public void AddView(View view) => _views.Add(view);
    
    public void DefineViews(IViewDefiner definer)
    {
        foreach (var v in _views)
        {
            definer.DefineView(v);
        }
    }
}