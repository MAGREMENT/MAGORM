namespace Base.Fields.Implementations;

public class DictionarySideEffectFieldCollection : SideEffectFieldCollection
{
    protected readonly Dictionary<string, object?> _dic = new();
    
    public override int GetFieldCount()
    {
        return _dic.Count;
    }

    public override IEnumerable<string> GetFieldsName()
    {
        return _dic.Keys;
    }

    protected override void InternalSetValue(string name, object? value)
    {
        _dic[name] = value;
    }

    protected override object? InternalGetValue(string name)
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