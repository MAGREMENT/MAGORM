using System.Data.Common;
using Core.Abstract;
using Core.Databases;
using Microsoft.Data.Sqlite;

namespace MICROSOFT.SQLite;

public class SQLiteDatabaseEngine(string connectionString) : CommonDatabaseEngine<SqliteConnection>
{
    protected override SqliteConnection CreateConnection()
    {
        return new SqliteConnection(connectionString);
    }

    protected override DbCommand CreateCommand(string query, SqliteConnection connection)
    {
        var cmd = connection.CreateCommand();
        cmd.CommandText = query;
        return cmd;
    }

    public override ITransaction CreateTransaction()
    {
        var con = CreateConnection();
        con.Open();
        return new SQLiteTransactionWrapper(con);
    }

    public override IReadOnlyList<ModelSpecification> GetModelSchemas() //TODO unique & fk contraints
    {
        var result = new List<ModelSpecification>();
        var fields = new List<FieldSpecification>();
        
        using var con = CreateConnection();
        con.Open();
        using var cmd = CreateCommand("SELECT name\n" + 
                                "FROM sqlite_schema\n" +
                                "WHERE type = \"table\" AND name NOT LIKE \"sqlite_%\";", con);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var name = reader.GetString(0);
            PrimaryKeySpecification? pk = null;

            using var pragmaCon = CreateConnection();
            pragmaCon.Open();
            using var pragmaCmd = CreateCommand($"PRAGMA table_info({name})", pragmaCon);

            using var pragmaReader = pragmaCmd.ExecuteReader();
            while (pragmaReader.Read())
            {
                var fieldName = pragmaReader.GetString(pragmaReader.GetOrdinal("name"));
                if (pragmaReader.GetBoolean(pragmaReader.GetOrdinal("pk")))
                {
                    if (pk is not null) throw new Exception(); //TODO
                    pk = new PrimaryKeySpecification(fieldName);
                }

                var type = pragmaReader.GetString(pragmaReader.GetOrdinal("type")) switch
                {
                    "INTEGER" => DBFieldType.INT,
                    "TEXT" => DBFieldType.STRING, //TODO handle bools
                    _ => throw new Exception() //TODO
                };

                var notNull = pragmaReader.GetBoolean(pragmaReader.GetOrdinal("notnull"));
                
                fields.Add(new FieldSpecification(fieldName, type, false, notNull, false));
            }

            if (pk is null) throw new Exception(); //TODO
            
            result.Add(new ModelSpecification(name, fields.ToArray(), pk, []));
            fields.Clear();
        }
        
        return result;
    }
}

public class SQLiteTransactionWrapper : CommonTransaction<SqliteConnection>
{
    private readonly SqliteTransaction _transaction;

    public SQLiteTransactionWrapper(SqliteConnection connection) : base(connection)
    {
        _transaction = connection.BeginTransaction();
    }

    protected override DbCommand CreateCommand(string query, SqliteConnection connection)
    {
        var cmd = connection.CreateCommand();
        cmd.CommandText = query;
        return cmd;
    }

    public override bool Commit()
    {
        _transaction.Commit();
        _transaction.Dispose();
        _transaction.Connection?.Dispose();

        return true;
    }
}