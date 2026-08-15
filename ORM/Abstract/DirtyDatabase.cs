using ORM.Queries;

namespace ORM.Abstract;

public class DirtyDatabase(IDatabaseEngine engine, IModelBank modelBank) : Database(engine, modelBank)
{
    private readonly DirtyCollection _dirty = new();
    
    public void NoticeDirty(IModel model, IRecord record, string field)
    {
        _dirty.AddToDirty(model, record, field);
    }
}

public class DirtyCollection : Dictionary<IRecord, (IModel, HashSet<string>)>
{
    public void AddToDirty(IModel model, IRecord record, string field)
    {
        if (!TryGetValue(record, out var data))
        {
            data = (model, new HashSet<string>());
            this[record] = data;
        }

        data.Item2.Add(field);
    }
}