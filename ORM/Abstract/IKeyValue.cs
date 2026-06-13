namespace ORM.Abstract;

public interface IKeyValue<in TKey, TValue>
{
    public bool TryGet(TKey key, out TValue value);
}

public class RecordDictionary : Dictionary<string, object?>, IKeyValue<string, object?>
{
    public bool TryGet(string key, out object? value)
        => TryGetValue(key, out value);
}