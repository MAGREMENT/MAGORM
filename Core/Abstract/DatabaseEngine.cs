namespace Core.Abstract;

public abstract class DatabaseEngine
{
    public abstract IQueryResult ExecuteResult(Query query, object[] parameters, object? context = null);
    public abstract object? ExecuteSingle(Query query, object[] parameters, object? context = null);
    public abstract void Execute(Query query, object[] parameters, object? context = null);
    public abstract void DoTransaction(OnTransaction onTransaction);

    protected static void CheckQueryParameters(Query query, object[] parameters)
    {
        if (query.ParameterCount != parameters.Length) throw new Exception("Wrong parameter count");
    }
}

public delegate void OnTransaction(DatabaseEngine engine, object? context);

public interface IQueryResult : IKeyValue<string, object?>, IKeyValue<int, object?>
{
    public int Length { get; }

    public bool Next();
}