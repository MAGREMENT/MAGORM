using ORM;
using ORM.Abstract;
using ORM.Languages;
using ORM.Languages.SQLite;
using ORM.RecordTypes;
using ORM.SQLite.Microsoft;

namespace Tests;

public class DatabaseTests
{
    private readonly List<Database> _databases = new();

    [SetUp]
    public void SetupDatabases()
    {
        _databases.Add(new Database(new SqLiteLanguage(), 
            new SQLiteDatabaseEngine("Data Source=test.db"), 
            new DictionaryModelBank()));
        
        foreach (var d in _databases)
        {
            d.Nuke();
        }
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
        var m1 = Models.DefineBase("Author",
            Fields.String("Name"));
            
        var m2 = Models.DefineBase("Books",
            Fields.String("Title", new FieldDefinitionsOptions
            {
                Required = true
            }),
            Fields.Int("PageCount"),
            Fields.Bool("IsProduced"),
            Fields.String("SerialNumber", new FieldDefinitionsOptions
            {
                Unique = true
            }),
            Fields.Reference("Author", "Author"));

        foreach (var db in _databases)
        {
            db.AddModels(m1);
            db.AddModels(m2);
            db.Sync();

            var r1 = db.GetModel("Author")!.Create<DictionarySideEffectRecord>(new RecordDictionary
            {
                {"Name", "Some Author"}
            });

            var r2 = db.GetModel("Books")!.Create<DictionarySideEffectRecord>(new RecordDictionary
            {
                {"Title", "Title Test"},
                {"PageCount", 8},
                {"IsProduced", true},
                {"SerialNumber", "123456"},
                {"Author", r1}
            });

            var r2Selected = db.GetModel("Books")!.Select<DictionarySideEffectRecord>();
            Assert.That(r2Selected, Has.Count.EqualTo(1));
            Assert.That(r2.AreCommonFieldsEqual(r2Selected[0]));

            r2Selected = db.GetModel("Books")!.Select<DictionarySideEffectRecord>(null, "Author.Name");
            Assert.That(r2Selected[0].Get("Author") is IRecord, Is.True);
            Assert.That(r2Selected[0]._<IRecord>("Author").Get("Name"), Is.EqualTo(r1.Get("Name")));
        }
    }
}