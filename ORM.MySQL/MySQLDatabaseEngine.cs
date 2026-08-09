using System.Data.Common;
using MySql.Data.MySqlClient;
using ORM.Languages;
using ORM.Queries;
using ORM.Queries.Common;
using ORM.Queries.Specifications;

namespace ORM.MySQL;

public class MySqlDatabaseEngine(string connectionString) : CommonDatabaseEngine<MySqlConnection>
{
    protected override MySqlConnection CreateConnection()
    {
        return new MySqlConnection(connectionString);
    }

    protected override DbCommand CreateCommand(string query, MySqlConnection connection)
    {
        return new MySqlCommand(query, connection);
    }

    public override ISqlLanguage Language => SQL.MySql;

    public override ITransaction CreateTransaction()
    {
        return new MySqlTransactionWrapper(CreateConnection());
    }

    public override IReadOnlyList<CreateSpecification> GetModelSchemas()
    {
        //TODO with var schema = connection.GetSchema("Tables");
        throw new NotImplementedException();
    }

    public override void DropAllTables()
    {
        //TODO
    }
}

public class MySqlTransactionWrapper : CommonTransaction<MySqlConnection>
{
    private readonly MySqlTransaction _transaction;

    public MySqlTransactionWrapper(MySqlConnection connection) : base(connection)
    {
        _transaction = connection.BeginTransaction();
    }

    protected override DbCommand CreateCommand(string query, MySqlConnection connection)
    {
        return new MySqlCommand(query, connection);
    }

    public override bool Commit()
    {
        _transaction.Commit();
        _transaction.Dispose();
        _transaction.Connection.Dispose();

        return true;
    }
}