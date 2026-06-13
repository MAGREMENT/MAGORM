using System.Data.Common;
using ORM.Queries.Results;
using ORM.Queries.Specifications;

namespace ORM.Queries.Common;

public abstract class CommonQueryExecutor<T> : IQueryExecutor
    where T : DbConnection
{
    protected abstract bool ShouldDisposeOfConnection { get; }

    protected abstract T CreateConnection();

    protected abstract DbCommand CreateCommand(string query, T connection);

    public void ExecuteBufferedResult(IBufferedQueryResult result, Query query)
    {
        var con = CreateConnection();
        con.Open();
        
        using (var cmd = SetupCommand(query, con))
        {
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.AddRow();
                result.Next();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    result.AddColumn(reader.GetName(i), reader.IsDBNull(i) ? null : reader.GetValue(i));
                }
            }
        }
        
        if (ShouldDisposeOfConnection) con.Dispose();

        if (!result.Reset()) throw new Exception("Could not reset result");
    }

    public TResult ExecuteResult<TResult>(OnQueryResult<TResult> onResult, Query query)
    {
        var con = CreateConnection();
        con.Open();

        TResult result;
        using (var cmd = SetupCommand(query, con))
        {
            using var queryResult = new DbDataReaderQueryResult(cmd.ExecuteReader());
            result = onResult(queryResult);
        }
        
        if(ShouldDisposeOfConnection) con.Dispose();

        return result;
    }

    public TResult ExecuteResult<TResult>(OnQueryResult<TResult> onResult, Query[] queries)
    {
        var con = CreateConnection();
        con.Open();
        
        var cmds = new DbCommand[queries.Length];
        for (int i = 0; i < queries.Length; i++)
        {
            cmds[i] = SetupCommand(queries[i], con);
        }

        TResult result;
        using (var queryResult = new MultiDbDataReaderQueryResult(cmds))
        {
            result = onResult(queryResult);
        }
        
        if(ShouldDisposeOfConnection) con.Dispose();

        return result;
    }

    public object? ExecuteSingle(Query query)
    {
        object? result;
        var con = CreateConnection();
        con.Open();
        
        using (var cmd = SetupCommand(query, con))
        {
            result = cmd.ExecuteScalar();
        }
        
        if(ShouldDisposeOfConnection) con.Dispose();
        return result;
    }

    public void Execute(Query query)
    {
        var con = CreateConnection();
        con.Open();

        using (var cmd = SetupCommand(query, con))
        {
            cmd.ExecuteNonQuery();
        }
        
        if(ShouldDisposeOfConnection) con.Dispose();
    }

    public void Execute(Query[] queries)
    {
        var con = CreateConnection();
        con.Open();

        foreach (var q in queries)
        {
            using var cmd = SetupCommand(q, con);
            cmd.ExecuteNonQuery();
        }
        
        if(ShouldDisposeOfConnection) con.Dispose();
    }

    private DbCommand SetupCommand(Query query, T con)
    {
        var cmd = CreateCommand(query.String, con);

        for (int i = 0; i < query.Parameters.Length; i++)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = "@" + i;
            p.Value = query.Parameters[i];
            cmd.Parameters.Add(p);
        }
        
        return cmd;
    }
}

public abstract class CommonDatabaseEngine<T>
    : CommonQueryExecutor<T>, IDatabaseEngine
    where T : DbConnection
{
    protected override bool ShouldDisposeOfConnection => true;
    
    public abstract ITransaction CreateTransaction();

    public abstract IReadOnlyList<CreateSpecification> GetModelSchemas();
    public abstract void DropAllTables(); //TODO should probably move part of this to the corresponding ISqlLanguage
}

public abstract class CommonTransaction<TConnection>(TConnection connection) 
    : CommonQueryExecutor<TConnection>, ITransaction
    where TConnection : DbConnection
{
    protected override bool ShouldDisposeOfConnection => false;

    protected override TConnection CreateConnection() => connection;

    public abstract bool Commit();
}