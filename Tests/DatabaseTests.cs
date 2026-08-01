using Base.Fields;
using ORM;
using ORM.Abstract;
using ORM.Languages;
using ORM.Languages.SQLite;
using ORM.RecordTypes;
using ORM.SQLite.Microsoft;
using MissingFieldException = ORM.Abstract.MissingFieldException;

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

            var r1 = m1.Create<DictionaryRecord>(new KeyValueDictionary
            {
                {"Name", "Some Author"}
            });

            var r2 = m2.Create<DictionaryRecord>(new KeyValueDictionary
            {
                {"Title", "Title Test"},
                {"PageCount", 8},
                {"IsProduced", true},
                {"SerialNumber", "123456"},
                {"Author", r1}
            });

            var r2Selected = m2.Select<DictionaryRecord>();
            Assert.That(r2Selected, Has.Count.EqualTo(1));
            Assert.That(r2.AreCommonFieldsEqual(r2Selected[0]));

            r2Selected = m2.Select<DictionaryRecord>(null, "Author.Name");
            Assert.That(r2Selected[0].Get("Author") is IRecord, Is.True);
            Assert.That(r2Selected[0]._<IRecord>("Author").Get("Name"), Is.EqualTo(r1.Get("Name")));
        }
    }

    [Test]
    public void CreateWithEmptyFieldTest()
    {
        var m1 = Models.DefineBase("Author",
            Fields.String("Name"));

        var m2 = Models.DefineBase("AuthorWithRequiredName",
            Fields.String("Name", new FieldDefinitionsOptions
            {
                Required = true
            }));

        foreach (var db in _databases)
        {
            db.AddModels(m1);
            db.AddModels(m2);
            db.Sync();

            var r1 = m1.Create<DictionaryRecord>(new KeyValueDictionary());
            var r1Selected = m1.Select<DictionaryRecord>();
            Assert.That(r1._<string>("Name"), Is.Null);
            Assert.That(r1Selected[0]._<string>("Name"), Is.Null);

            Assert.Throws<MissingFieldException>(() => m2.Create<DictionaryRecord>(new KeyValueDictionary()));
        }
    }
}