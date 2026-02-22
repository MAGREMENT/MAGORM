namespace Core.Abstract;

public interface IQueryExecutor
{
    public IQueryResult ExecuteResult(Query query, object[] parameters);
    public object? ExecuteSingle(Query query, object[] parameters);
    public void Execute(Query query, object[] parameters);
}

public interface IQueryResult : IKeyValue<string, object?>, IKeyValue<int, object?>
{
    public int Length { get; }

    public bool Next();
}

public interface ITransaction : IQueryExecutor
{
    public bool Commit();
}

public interface IDatabaseEngine : IQueryExecutor
{
    public ITransaction CreateTransaction();
}

