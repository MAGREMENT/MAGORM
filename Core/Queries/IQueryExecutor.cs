namespace Core.Queries;

public interface IQueryExecutor
{
    public void ExecuteBufferedResult(IBufferedQueryResult result, Query query, IReadOnlyList<object> parameters);
    public T ExecuteResult<T>(OnQueryResult<T> onResult, Query query, IReadOnlyList<object> parameters);
    public object? ExecuteSingle(Query query, IReadOnlyList<object> parameters);
    public void Execute(Query query, IReadOnlyList<object> parameters);
}

public delegate T OnQueryResult<out T>(IQueryResult result);

public interface ITransaction : IQueryExecutor
{
    public bool Commit();
}

public interface IDatabaseEngine : IQueryExecutor
{
    public ITransaction CreateTransaction();

    public IReadOnlyList<ModelSpecification> GetModelSchemas();

    public void DropAllTables();
}

