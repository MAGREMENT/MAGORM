using Core.Abstract;

namespace Core.Queries;

public interface IQueryResult : IKeyValue<string, object?>, IKeyValue<int, object?>, IDisposable
{
    public bool Next();
    public bool Reset();
}

public interface IBufferedQueryResult : IQueryResult
{
    public void AddRow();

    public void AddColumn(string name, object? value);
}