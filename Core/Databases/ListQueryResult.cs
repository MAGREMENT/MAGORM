using Core.Abstract;

namespace Core.Databases;

public class ListQueryResult : IQueryResult
{
    private readonly List<DictionaryList<string, object?>> _values;
    private int _curr = -1;

    public int Length => _values.Count;

    public void AddRow()
    {
        _values.Add(new DictionaryList<string, object?>());
    }

    public void AddColumn(string name, object? value)
    {
        _values[_curr].Add(name, value);
    }
    
    public bool TryGetValue(string key, out object? value)
    {
        return _values[_curr].TryGetValue(key, out value);
    }

    public bool TryGetValue(int key, out object? value)
    {
        return _values[_curr].TryGetValue(key, out value);
    }
    
    public bool Next()
    {
        _curr++;
        return _curr < _values.Count;
    }

    public void Reset()
    {
        _curr = -1;
    }
}

public class DictionaryList<TKey, TValue> : IKeyValue<TKey, TValue>, IKeyValue<int, TValue> 
    where TKey : notnull
{
    private readonly Dictionary<TKey, int> _dic = new();
    private readonly List<TValue> _list = new();

    public void Add(TKey key, TValue value)
    {
        if (_dic.TryAdd(key, _list.Count)) _list.Add(value);
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        if (!_dic.TryGetValue(key, out var i))
        {
            value = default!;
            return false;
        }
        
        value = _list[i];
        return true;
    }

    public bool TryGetValue(int key, out TValue value)
    {
        if (key >= _list.Count)
        {
            value = default!;
            return false;
        }

        value = _list[key];
        return true;
    }
}