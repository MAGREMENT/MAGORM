using ORM.ModelTypes;

namespace ORM.Abstract;

//TODO cached database
public interface IRecordCache
{
    void SetRecord(IModel bufferedModel, IRecord record);
    bool TryGetRecord(IModel bufferedModel, object primaryKey, out IRecord? record);
    IEnumerable<IRecord> EnumerateRecords(IModel bufferedModel);
}

//TODO Simple dictionary is probably better
public class DoubleDictionaryRecordCache : Dictionary<IModel, Dictionary<object, IRecord>>, IRecordCache
{
    public void SetRecord(IModel bufferedModel, IRecord record)
    {
        if (!TryGetValue(bufferedModel, out var dic))
        {
            dic = new Dictionary<object, IRecord>();
            this[bufferedModel] = dic;
        }

        if (!record.TryGet(bufferedModel.GetPrimaryKey().Name, out var pkValue) || pkValue is null)
            throw new Exception("Need a value for the primary key to add the record into the cache");
        dic[pkValue] = record;
    }

    public bool TryGetRecord(IModel bufferedModel, object primaryKey, out IRecord? record)
    {
        if (!TryGetValue(bufferedModel, out var dic))
        {
            record = null!;
            return false;
        }

        return dic.TryGetValue(primaryKey, out record);
    }

    public IEnumerable<IRecord> EnumerateRecords(IModel bufferedModel)
    {
        if (!TryGetValue(bufferedModel, out var dic)) return [];

        return dic.Values;
    }
}