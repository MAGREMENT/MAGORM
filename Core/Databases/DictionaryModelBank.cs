using System.Collections;
using Core.Abstract;

namespace Core.Databases;

public class DictionaryModelBank : IModelBank
{
    private readonly Dictionary<string, Model> _dictionary = new();
    private readonly Dictionary<Type, string> _names = new();
    
    public Model? GetModel<TModel>()
    {
        if (!_names.TryGetValue(typeof(TModel), out var n)) return null;
        return GetModel(n);
    }

    public Model? GetModel(string name)
    {
        return _dictionary.GetValueOrDefault(name, null!);
    }

    public void AddModels(IEnumerable<Model> models)
    {
        foreach (var model in models)
        {
            _dictionary.Add(model.Name, model);
            _names.Add(model.GetType(), model.Name);
        }
    }

    public IEnumerator<Model> GetEnumerator() => _dictionary.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}