namespace ORM.Abstract;

public interface IRecordCache
{
    void SetRecord(Model model, IRecord record);
    bool TryGetRecord(Model model, object primaryKey, out IRecord record);
    IEnumerable<IRecord> EnumerateRecords(Model model);
}

public class DictionaryRecordCache : Dictionary<Model, Dictionary<object, IRecord>>, IRecordCache
{
    public void SetRecord(Model model, IRecord record)
    {
        if (!TryGetValue(model, out var dic))
        {
            dic = new Dictionary<object, IRecord>();
            this[model] = dic;
        }

        if (!record.TryGet(model.GetPrimaryKey().Name, out var pkValue) || pkValue is null)
            throw new Exception("Need a value for the primary key to add the record into the cache");
        dic[pkValue] = record;
    }

    public bool TryGetRecord(Model model, object primaryKey, out IRecord record)
    {
        if (!TryGetValue(model, out var dic))
        {
            record = null!;
            return false;
        }

        return dic.TryGetValue(primaryKey, out record);
    }

    public IEnumerable<IRecord> EnumerateRecords(Model model)
    {
        if (!TryGetValue(model, out var dic)) return [];

        return dic.Values;
    }
}