using ORM.Abstract;

namespace ORM.Languages;

public class DictionaryModelBank : Dictionary<string, IModel>, IModelBank
{
    public IModel? GetModel(string name)
    {
        return this.GetValueOrDefault(name, null!);
    }

    public void AddModels(IEnumerable<IModel> models)
    {
        foreach (var model in models)
        {
            Add(model.Name, model);
        }
    }

    public IEnumerable<IModel> EnumerateModels() => Values;
}