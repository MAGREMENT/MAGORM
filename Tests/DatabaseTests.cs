using ORM;
using ORM.Abstract;
using ORM.Languages;
using ORM.Queries.Builder;
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
        _databases.Add(new Database(
            new SQLiteDatabaseEngine("Data Source=test.db"), 
            new DictionaryModelRegistry()));
        
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

            var r1 = db.InsertRecord(m1, new DictionaryRecord
            {
                {"Name", "Some Author"}
            });

            var r2 = db.InsertRecord(m2, new DictionaryRecord
            {
                {"Title", "Title Test"},
                {"PageCount", 8},
                {"IsProduced", true},
                {"SerialNumber", "123456"},
                {"Author", r1}
            });

            var r2Selected = db.SelectRecords<DictionaryRecord>(m2, null);
            Assert.That(r2Selected, Has.Count.EqualTo(1));
            Assert.That(r2Selected[0].GetFieldCount(), Is.EqualTo(6));
            foreach (var fName in new[] { "Title", "PageCount", "IsProduced", "SerialNumber" })
            {
                Assert.That(r2Selected[0].Get(fName), Is.EqualTo(r2.Get(fName)));
            }

            var refFieldValue = r2Selected[0].Get("Author");
            Assert.That(refFieldValue, Is.AssignableTo<IRecord>());
            Assert.That(((IRecord)refFieldValue).Get("Id"), Is.EqualTo(r1.Get("Id")));

            r2Selected = db.SelectRecords<DictionaryRecord>(m2, ["Author.Name"], null);
            Assert.That(r2Selected[0].Get("Author") is IRecord, Is.True);
            Assert.That(r2Selected[0]._<IRecord>("Author").Get("Name"), Is.EqualTo(r1.Get("Name")));

            r2.Set("PageCount", 12);
            r2Selected = db.SelectRecords<DictionaryRecord>(m2, ["PageCount"], null);
            Assert.That(r2.Get("PageCount"), Is.EqualTo(12));
            //Not yet updated
            Assert.That(r2Selected[0].Get("PageCount"), Is.EqualTo(8));
            
            db.UpdateRecords([new RecordUpdate(m2, r2, ["PageCount"])]);
            r2Selected = db.SelectRecords<DictionaryRecord>(m2, ["PageCount"], null);
            Assert.That(r2Selected[0].Get("PageCount"), Is.EqualTo(12));
            
            db.DeleteRecords([new RecordDelete(m2, [r2])]);
            r2Selected = db.SelectRecords<DictionaryRecord>(m2, null);
            Assert.That(r2Selected, Has.Count.EqualTo(0));
        }
    }

    [Test]
    public void InsertWithEmptyFieldTest()
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

            var r1 = db.InsertRecord(m1, new DictionaryRecord());
            var r1Selected = db.SelectRecords<DictionaryRecord>(m1, null);
            Assert.That(r1Selected[0]._<int>("Id"), Is.EqualTo(r1._<int>("Id")));
            Assert.That(r1.TryGet("Name", out _), Is.False);

            Assert.Throws<MissingFieldException>(() => db.InsertRecord(m2, new DictionaryRecord()));
        }
    }
}