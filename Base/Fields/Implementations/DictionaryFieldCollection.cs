namespace Base.Fields.Implementations;

public class DictionaryFieldCollection : IFieldCollection
{
    protected readonly Dictionary<string, object?> _dic = new();
    
    public int GetFieldCount()
    {
        return _dic.Count;
    }

    public IEnumerable<string> GetFieldsName()
    {
        return _dic.Keys;
    }

    public void Set(string name, object? value)
    {
        _dic[name] = value;
    }

    public object? Get(string name)
    {
        return _dic[name];
    }
    
    public T _<T>(string name)
    {
        var v = _dic[name];
        return v is null ? default! : (T)v;
    }

    public object? _(string name)
    {
        return _dic[name];
    }
}