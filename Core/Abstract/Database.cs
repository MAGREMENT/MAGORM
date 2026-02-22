namespace Core.Abstract;

public class Database
{
    private readonly IQueryBuilder _queryBuilder;
    private readonly IDatabaseEngine _engine;
    private readonly IModelBank _modelBank;
    private readonly DirtyCollection _dirty = new();

    public Database(IQueryBuilder queryBuilder, IDatabaseEngine engine, IModelBank modelBank)
    {
        _queryBuilder = queryBuilder;
        _engine = engine;
        _modelBank = modelBank;
    }

    public void AddModels(params Model[] models) => AddModels((IEnumerable<Model>) models);

    public void AddModels(IEnumerable<Model> models)
    {
        _modelBank.AddModels(models);
        foreach (var model in models)
        {
            model.AttachToDatabase(this);
        }
    }

    public void Sync()
    {
        var currentSpecifications = _engine.GetModelSchemas();
        var desiredSpecifications = new List<ModelSpecification>();

        foreach (var m in _modelBank)
        {
            desiredSpecifications.Add(m.GenerateSpecification());
        }
    }

    public Model? GetModel<TModel>() => _modelBank.GetModel<TModel>();
    public Model? GetModel(string name) => _modelBank.GetModel(name);
}

public interface IDatabaseAttachable
{
    void AttachToDatabase(Database database);
}

public interface IDirtyCollection
{
    void AddToDirty(Model model, IRecord record);
}

public class DirtyCollection : Dictionary<string, Dictionary<object, IRecord>>, IDirtyCollection
{
    public void AddToDirty(Model model, IRecord record)
    {
        if (!TryGetValue(model.Name, out var dic))
        {
            dic = new Dictionary<object, IRecord>();
            this[model.Name] = dic;
        }

        dic.TryAdd(record.GetFieldValue(model.GetPrimaryKey().Name), record);
    }
}