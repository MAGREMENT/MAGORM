namespace Base.Fields;

public interface IReadOnlyKeyValue<in TKey, TValue>
{
    bool TryGet(TKey key, out TValue value);

    TValue Get(TKey key);
}

public interface IKeyValue<in TKey, TValue> : IReadOnlyKeyValue<TKey, TValue>
{
    void Set(TKey key, TValue value);
}

public static class KeyValue
{
    public static TValue DefaultGet<TKey, TValue>(IReadOnlyKeyValue<TKey, TValue> kv, TKey key)
    {
        return !kv.TryGet(key, out var val) ? throw new KeyNotFoundException() : val;
    }
}

public class KeyValueDictionary : Dictionary<string, object?>, IKeyValue<string, object?>
{
    public bool TryGet(string key, out object? value) => TryGetValue(key, out value);

    public object? Get(string name) => this[name];

    public void Set(string name, object? value) => this[name] = value;
}