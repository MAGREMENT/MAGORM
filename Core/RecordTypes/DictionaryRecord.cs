using Core.Abstract;

namespace Core.RecordTypes;

public class DictionaryRecord : Record
{
    private readonly Dictionary<string, object> _dic = new();
    
    protected override void InternalSetFieldValue(string name, object value)
    {
        _dic[name] = value;
    }

    protected override bool HasValue(string name)
    {
        return _dic.ContainsKey(name);
    }

    public override object GetFieldValue(string name)
    {
        return _dic[name];
    }

    public T _<T>(string name)
    {
        return (T)_dic[name];
    }
}