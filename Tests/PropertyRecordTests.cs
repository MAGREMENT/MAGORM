using ORM.RecordTypes;

namespace Tests;

public class PropertyRecordTests
{
    [Test]
    public void Test()
    {
        var book = new Book();
        var model = book.GetModel();
        Assert.That(model, Is.Not.Null);
        Assert.That(model.AllFieldDefinitions, Has.Count.EqualTo(3));
    }
}

[ModelDefinition]
public partial class Book : PropertyRecord
{
    [ModelField(Primary = true, AutoIncrement = true)]
    public int Id { get; set; }
    
    [ModelField(Required = true)]
    public string Name { get; set; } = string.Empty;
    
    [ModelField(Unique = true)]
    public int Number { get; set; }
    
    public string? NotStored { get; set; }
}