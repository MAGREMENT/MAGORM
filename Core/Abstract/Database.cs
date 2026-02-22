namespace Core.Abstract;

public class Database
{
    private readonly IQueryBuilder _queryBuilder;
    private readonly IDatabaseEngine _engine;
    private readonly IModelBank _modelBank;
    
    private readonly Dictionary<string, Dictionary<object, IRecord>> _dirty = new();

    protected Database(IQueryBuilder queryBuilder, IDatabaseEngine engine, IModelBank modelBank)
    {
        _queryBuilder = queryBuilder;
        _engine = engine;
        _modelBank = modelBank;
    }

    public void AddToDirty(AbstractModel model, IRecord record)
    {
        if (!_dirty.TryGetValue(model.Name, out var dic))
        {
            dic = new Dictionary<object, IRecord>();
            _dirty[model.Name] = dic;
        }

        dic.TryAdd(record.GetFieldValue(model.GetPrimaryKey().Name), record);
    }
}

public interface IModelBank
{
    protected AbstractModel? GetModel<TModel>();
    protected AbstractModel? GetModel(string name);
}