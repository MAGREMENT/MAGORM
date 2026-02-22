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
        return new SQLiteTransactionWrapper(CreateConnection());
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