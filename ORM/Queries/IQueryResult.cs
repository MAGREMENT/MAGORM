using ORM.Abstract;

namespace ORM.Queries;

public interface IQueryResult : IKeyValue<string, object?>, IKeyValue<int, object?>, IDisposable
{
    public bool Next();
    public bool Reset();
    public bool NextResultSet();
}

public interface IBufferedQueryResult : IQueryResult
{
    public void AddRow();

    public void AddColumn(string name, object? value);
}