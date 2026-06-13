using ORM.Queries.Specifications;

namespace ORM.Queries;

public interface IQueryExecutor
{
    public void ExecuteBufferedResult(IBufferedQueryResult result, Query query);
    public T ExecuteResult<T>(OnQueryResult<T> onResult, Query query);
    public T ExecuteResult<T>(OnQueryResult<T> onResult, Query[] queries);
    public object? ExecuteSingle(Query query);
    public void Execute(Query query);
    public void Execute(Query[] queries);
}

public delegate T OnQueryResult<out T>(IQueryResult result);

public interface ITransaction : IQueryExecutor
{
    public bool Commit();
}

public interface IDatabaseEngine : IQueryExecutor
{
    public ITransaction CreateTransaction();

    public IReadOnlyList<CreateSpecification> GetModelSchemas();

    public void DropAllTables();
}

