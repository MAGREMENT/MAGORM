using System.Data.Common;

namespace Core.Queries.Results;

public class DbDataReaderQueryResult(DbDataReader _reader) : IQueryResult
{
    public void Dispose()
    {
        _reader.Dispose();
    }

    public bool TryGetValue(string key, out object? value)
    {
        return TryGetValue(_reader.GetOrdinal(key), out value);
    }

    public bool TryGetValue(int key, out object? value)
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
}