using ORM.Abstract;
using ORM.ModelTypes;

namespace ORM.Languages;

public class DictionaryModelRegistry : ModelRegistry
{
    private readonly Dictionary<string, IModel> _dic = new();

    protected override void AddModelsInternal(IEnumerable<IModel> models)
    {
        foreach (var model in models)
        {
            _dic.Add(model.Name, model);
        }
    }

    public override IModel? GetModel(string name)
    {
        return _dic.GetValueOrDefault(name, null!);
    }

    public override IEnumerable<IModel> EnumerateModels() => _dic.Values;
}