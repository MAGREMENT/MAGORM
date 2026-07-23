using Base.Fields;

namespace ORM.Queries.Results;

public class ListQueryResult : IBufferedQueryResult
{
    private readonly List<DictionaryList<string, object?>> _values = new();
    private int _curr = -1;

    public int Length => _values.Count;

    public void AddRow()
    {
        _values.Add(new DictionaryList<string, object?>());
    }

    public void AddColumn(string name, object? value)
    {
        _values[_curr].Set(name, value);
    }
    
    public bool TryGet(string key, out object? value)
    {
        return _values[_curr].TryGet(key, out value);
    }

    public object? Get(string name)
    {
        return _values[_curr].Get(name);
    }

    public bool TryGet(int key, out object? value)
    {
        return _values[_curr].TryGet(key, out value);
    }

    public object? Get(int name)
    {
        return _values[_curr].Get(name);
    }

    public bool Next()
    {
        _curr++;
        return _curr < _values.Count;
    }

    public bool Reset()
    {
        _curr = -1;
        return true;
    }

    public bool NextResultSet()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        
    }
}

public class DictionaryList<TKey, TValue> : IKeyValue<TKey, TValue>
    where TKey : notnull
{
    private readonly Dictionary<TKey, int> _dic = new();
    private readonly List<TValue> _list = new();

    public void Set(TKey key, TValue value)
    {
        if (_dic.TryAdd(key, _list.Count)) _list.Add(value);
    }

    public bool TryGet(TKey key, out TValue value)
    {
        if (!_dic.TryGetValue(key, out var i))
        {
            value = default!;
            return false;
        }
        
        value = _list[i];
        return true;
    }

    public TValue Get(TKey name) => KeyValue.DefaultGet(this, name);

    public bool TryGet(int key, out TValue value)
    {
        if (key >= _list.Count)
        {
            value = default!;
            return false;
        }

        value = _list[key];
        return true;
    }

    public TValue Get(int name)
    {
        return !TryGet(name, out var val) ? throw new KeyNotFoundException() : val;
    }
}