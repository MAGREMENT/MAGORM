using System.Data.Common;
using Base.Fields;

namespace ORM.Queries.Results;

public abstract class DbDataReaderQueryResult(DbDataReader reader) : IQueryResult
{
    protected DbDataReader _reader = reader;
    
    public bool TryGet(string key, out object? value)
    {
        return TryGet(_reader.GetOrdinal(key), out value);
    }
    
    public object? Get(string name) => KeyValue.DefaultGet<string, object?>(this, name);

    public bool TryGet(int key, out object? value)
    {
        if (key < 0 || key >= _reader.FieldCount)
        {
            value = null;
            return false;
        }
        
        value = _reader.IsDBNull(key) ? null : _reader.GetValue(key);
        return true;
    }

    public object? Get(int name) => KeyValue.DefaultGet<int, object?>(this, name);
    
    public bool Reset()
    {
        return false;
    }

    public abstract void Dispose();

    public abstract bool Next();

    public abstract bool NextResultSet();
    
    public bool TryGetNullableInt(string name, out int? value)
    {
        if (!KeyCheck(name, out var key))
        {
            value = 0;
            return false;
        }

        value = _reader.IsDBNull(key) ? null : _reader.GetInt32(key);
        return true;
    }

    public bool TryGetInt(string name, out int value)
    {
        if (!KeyCheck(name, out var key))
        {
            value = 0;
            return false;
        }

        value = _reader.GetInt32(key);
        return true;
    }

    public bool TryGetNullableString(string name, out string? value)
    {
        if (!KeyCheck(name, out var key))
        {
            value = null!;
            return false;
        }
        
        value = _reader.IsDBNull(key) ? null : _reader.GetString(key);
        return true;
    }

    public bool TryGetString(string name, out string value)
    {
        if (!KeyCheck(name, out var key))
        {
            value = null!;
            return false;
        }
        
        value = _reader.GetString(key);
        return true;
    }

    public bool TryGetNullableBool(string name, out bool? value)
    {
        if (!KeyCheck(name, out var key))
        {
            value = false;
            return false;
        }
        
        value = _reader.IsDBNull(key) ? null :_reader.GetBoolean(key);
        return true;
    }

    public bool TryGetBool(string name, out bool value)
    {
        if (!KeyCheck(name, out var key))
        {
            value = false;
            return false;
        }
        
        value = _reader.GetBoolean(key);
        return true;
    }

    private bool KeyCheck(string name, out int key)
    {
        key = _reader.GetOrdinal(name);
        return key >= 0 && key < _reader.FieldCount;
    }
}

public class SingleDbDataReaderQueryResult(DbDataReader reader) : DbDataReaderQueryResult(reader)
{
    public override void Dispose()
    {
        _reader.Dispose();
    }

    public override bool Next()
    {
        return _reader.Read();
    }

    public override bool NextResultSet()
    {
        return _reader.NextResult();
    }
}

public class MultiDbDataReaderQueryResult : DbDataReaderQueryResult
{
    private readonly DbCommand[] _commands;
    private int _currIndex = -1;

    public MultiDbDataReaderQueryResult(DbCommand[] commands) : base(null!)
    {
        _commands = commands;
        NextReader();
    }
    
    public override void Dispose()
    {
        _reader.Dispose();
        foreach(var c in _commands) c.Dispose();
    }

    public override bool Next()
    {
        return _currIndex >= 0 && _currIndex < _commands.Length && _reader.Read();
    }

    public override bool NextResultSet()
    {
        _reader.Dispose();
        NextReader();

        return _currIndex < _commands.Length;
    }

    private void NextReader()
    {
        while (_currIndex < _commands.Length - 1)
        {
            _reader = _commands[++_currIndex].ExecuteReader();
            if (_reader.FieldCount > 0) return;
            
            _reader.Dispose();
        }

        _currIndex = _commands.Length;
    }
}