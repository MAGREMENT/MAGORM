using Base.Fields;

namespace ORM.Queries;

public interface IQueryResult : IReadOnlyKeyValue<string, object?>, IReadOnlyKeyValue<int, object?>, IDisposable
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