using Core.Abstract;
using Core.Databases;
using MySql.Data.MySqlClient;

namespace ADO.NET.MYSQL;

public class MySqlDatabaseEngine : DatabaseEngine
{
    private readonly string _connectionString;

    public MySqlDatabaseEngine(string connectionString)
    {
        _connectionString = connectionString;
    }

    public override IQueryResult ExecuteResult(Query query, object[] parameters, object? context = null)
    {
        CheckQueryParameters(query, parameters);

        var result = new ListQueryResult();
        var con = context as MySqlConnection ?? new MySqlConnection(_connectionString);
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
        
        con.Dispose();
        return result;
    }

    public override object? ExecuteSingle(Query query, object[] parameters, object? context = null)
    {
        CheckQueryParameters(query, parameters);

        object? result;
        var con = context as MySqlConnection ?? new MySqlConnection(_connectionString);
        con.Open();
        
        using (var cmd = SetupCommand(query, parameters, con))
        {
            result = cmd.ExecuteScalar();
        }
        
        con.Dispose();
        return result;
    }

    public override void Execute(Query query, object[] parameters, object? context = null)
    {
        CheckQueryParameters(query, parameters);
        
        var con = context as MySqlConnection ?? new MySqlConnection(_connectionString);
        con.Open();

        using (var cmd = SetupCommand(query, parameters, con))
        {
            cmd.ExecuteNonQuery();
        }
        
        con.Dispose();
    }

    private MySqlCommand SetupCommand(Query query, object[] parameters, MySqlConnection con)
    {
        var cmd = new MySqlCommand(query.String, con);

        for (int i = 0; i < parameters.Length; i++)
        {
            cmd.Parameters.AddWithValue("@" + i, parameters[i]);
        }
        
        return cmd;
    }

    public override void DoTransaction(OnTransaction onTransaction)
    {
        using var con = new MySqlConnection(_connectionString);
        con.Open();

        using var transaction = con.BeginTransaction();
        onTransaction(this, con);
        
        transaction.Commit();
    }
}