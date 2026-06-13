using System.Data.Common;

namespace ORM.Queries.Results;

public class DbDataReaderQueryResult(DbDataReader _reader) : IQueryResult
{
    public void Dispose()
    {
        _reader.Dispose();
    }

    public bool TryGet(string key, out object? value)
    {
        return TryGet(_reader.GetOrdinal(key), out value);
    }

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

    public bool Next()
    {
        return _reader.Read();
    }

    public bool Reset()
    {
        return false;
    }

    public bool NextResultSet()
    {
        return _reader.NextResult();
    }
}

public class MultiDbDataReaderQueryResult : IQueryResult
{
    private DbDataReader _currReader = null!;
    private readonly DbCommand[] _commands;
    private int _currIndex = -1;

    public MultiDbDataReaderQueryResult(DbCommand[] commands)
    {
        _commands = commands;
        NextReader();
    }
    
    public void Dispose()
    {
        _currReader.Dispose();
        foreach(var c in _commands) c.Dispose();
    }

    public bool TryGet(string key, out object? value)
    {
        return TryGet(_currReader.GetOrdinal(key), out value);
    }

    public bool TryGet(int key, out object? value)
    {
        if (key < 0 || key >= _currReader.FieldCount)
        {
            value = null;
            return false;
        }
        
        value = _currReader.IsDBNull(key) ? null : _currReader.GetValue(key);
        return true;
    }

    public bool Next()
    {
        return _currIndex >= 0 && _currIndex < _commands.Length && _currReader.Read();
    }

    public bool Reset()
    {
        return false;
    }

    public bool NextResultSet()
    {
        _currReader.Dispose();
        NextReader();

        return _currIndex < _commands.Length;
    }

    private void NextReader()
    {
        while (_currIndex < _commands.Length - 1)
        {
            _currReader = _commands[++_currIndex].ExecuteReader();
            if (_currReader.HasRows) return;
            
            _currReader.Dispose();
        }

        _currIndex = _commands.Length;
    }
}