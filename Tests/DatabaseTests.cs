using Core;
using Core.Abstract;
using Core.Databases;
using Core.Databases.SQLite;
using Core.RecordTypes;
using MICROSOFT.SQLite;

namespace Tests;

public class DatabaseTests
{
    private readonly List<Database> _databases = new();

    [SetUp]
    public void SetupDatabases()
    {
        _databases.Add(new Database(new SQLiteQueryBuilder(), 
            new SQLiteDatabaseEngine("Data Source=test.db"), 
            new DictionaryModelBank()));
    }

    [TearDown]
    public void TeardownDatabases()
    {
        foreach (var d in _databases)
        {
            d.Nuke();
        }
        _databases.Clear();
    }

    [Test]
    public void BasicScenarioTest1()
    {
        var m1 = Models.DefineBase("Books",
            Fields.String("Title", new FieldDefinitionsOptions
            {
                Required = true
            }),
            Fields.Int("PageCount"),
            Fields.Bool("IsProduced"),
            Fields.String("SerialNumber", new FieldDefinitionsOptions
            {
                Unique = true
            }));

        foreach (var db in _databases)
        {
            db.AddModels(m1);
            db.Sync();

            var r1 = db.GetModel("Books")!.Create<DictionaryRecord>(new Dictionary<string, object>
            {
                {"Title", "Title Test"},
                {"PageCount", 8},
                {"IsProduced", true},
                {"SerialNumber", "123456"}
            });
        }
    }
}