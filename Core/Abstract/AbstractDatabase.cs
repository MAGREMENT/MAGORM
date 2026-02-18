namespace Core.Abstract;

public abstract class AbstractDatabase
{
    private readonly Dictionary<string, Dictionary<object, IRecord>> _dirty = new();

    public void AddToDirty(AbstractModel model, IRecord record)
    {
        if (!_dirty.TryGetValue(model.Name, out var dic))
        {
            dic = new Dictionary<object, IRecord>();
            _dirty[model.Name] = dic;
        }

        dic.TryAdd(record.GetFieldValue(model.GetPrimaryKey().Name), record);
    }
    
    protected abstract AbstractModel? GetModel<TModel>();
    protected abstract AbstractModel? GetModel(string name);
}