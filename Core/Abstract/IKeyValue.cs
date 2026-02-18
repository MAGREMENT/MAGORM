namespace Core.Abstract;

public interface IKeyValue<in TKey, TValue>
{
    public bool TryGetValue(TKey key, out TValue value);
}