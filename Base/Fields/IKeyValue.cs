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