using Base.Fields.Implementations;

namespace Tests;

public class PropertyFieldCollectionTest
{
    [Test]
    public void Test()
    {
        var cls = new TestClass();
        Assert.That(cls.GetFieldCount(), Is.EqualTo(2));
        Assert.That(cls.GetFieldsName(), Does.Contain(nameof(TestClass.SomeNumber)));
        Assert.That(cls.GetFieldsName(), Does.Contain(nameof(TestClass.SomeString)));

        cls.SomeNumber = 3;
        Assert.That(cls.Get(nameof(TestClass.SomeNumber)), Is.EqualTo(3));
        cls.Set(nameof(TestClass.SomeNumber), 12);
        Assert.That(cls.SomeNumber, Is.EqualTo(12));
    }
}

[PropertyFieldCollection]
public partial class TestClass : PropertyFieldCollection
{
    [Field]
    public int SomeNumber { get; set; }

    [Field] 
    public string SomeString { get; set; } = string.Empty;
}