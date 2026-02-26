using System.Data.Common;
using Core.Abstract;

namespace Core.Databases;

public abstract class CommonQueryExecutor<T> : IQueryExecutor
    where T : DbConnection
{
    protected abstract bool ShouldDisposeOfConnection { get; }

    protected abstract T CreateConnection();

    protected abstract DbCommand CreateCommand(string query, T connection);
    

    public IQueryResult ExecuteResult(Query query, object[] parameters)
    {
        query.CheckParameters(parameters);

        var result = new ListQueryResult();
        var con = CreateConnection();
        con.Open();
        
        using (var cmd = SetupCommand(query, parameters, con))
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
        
        if(ShouldDisposeOfConnection) con.Dispose();
        return result;
    }

    public object? ExecuteSingle(Query query, object[] parameters)
    {
        query.CheckParameters(parameters);

        object? result;
        var con = CreateConnection();
        con.Open();
        
        using (var cmd = SetupCommand(query, parameters, con))
        {
            result = cmd.ExecuteScalar();
        }
        
        if(ShouldDisposeOfConnection) con.Dispose();
        return result;
    }

    public void Execute(Query query, object[] parameters)
    {
        query.CheckParameters(parameters);

        var con = CreateConnection();
        con.Open();

        using (var cmd = SetupCommand(query, parameters, con))
        {
            cmd.ExecuteNonQuery();
        }
        
        if(ShouldDisposeOfConnection) con.Dispose();
    }

    private DbCommand SetupCommand(Query query, object[] parameters, T con)
    {
        var cmd = CreateCommand(query.String, con);

        for (int i = 0; i < parameters.Length; i++)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = "@" + i;
            p.Value = parameters[i];
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

    public abstract IReadOnlyList<ModelSpecification> GetModelSchemas();
    public abstract void DropAllTables();
}

public abstract class CommonTransaction<TConnection>(TConnection connection) 
    : CommonQueryExecutor<TConnection>, ITransaction
    where TConnection : DbConnection
{
    protected override bool ShouldDisposeOfConnection => false;

    protected override TConnection CreateConnection() => connection;

    public abstract bool Commit();
}