namespace Base.Fields.Implementations;

public class DictionaryFieldCollection : Dictionary<string, object?>, IFieldCollection
{
    public int GetFieldCount()
    {
        return Count;
    }

    public IEnumerable<string> GetFieldsName()
    {
        return Keys;
    }

    public void Set(string name, object? value)
    {
        this[name] = value;
    }

    public bool TryGet(string key, out object? value)
    {
        return TryGetValue(key, out value);
    }

    public object? Get(string name)
    {
        return this[name];
    }
    
    public T _<T>(string name)
    {
        var v = this[name];
        return v is null ? default! : (T)v;
    }

    public object? _(string name)
    {
        return this[name];
    }
}