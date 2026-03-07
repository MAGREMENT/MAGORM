using Core.Abstract;

namespace Core.RecordTypes;

public class DictionaryTrackedRecord : TrackedRecord
{
    private readonly Dictionary<string, object?> _dic = new();
    
    protected override void InternalSetValue(string name, object? value)
    {
        _dic[name] = value;
    }

    public override int GetFieldCount() => _dic.Count;

    public override IEnumerable<string> GetFieldNames() => _dic.Keys;

    public override bool TryGetValue(string key, out object? value)
    {
        return _dic.TryGetValue(key, out value);
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