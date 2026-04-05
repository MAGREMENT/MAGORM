using Base.PropertyCollections;
using Base.PropertyCollections.Implementations;

namespace Tests;

public class PropertyCollectionGeneratorTests
{
    [Test]
    public void Test()
    {
        var cls = new TestClassPropertyCollection();
        var module = new CountModule();
        cls.InsertModule(module);
        
        cls.Value = 67;
        var got = cls.Value;
        
        Assert.That(got, Is.EqualTo(67));
        Assert.That(module.BeforeSetCount, Is.EqualTo(1));
        Assert.That(module.AfterSetCount, Is.EqualTo(1));
        Assert.That(module.BeforeGetCount, Is.EqualTo(1));
        Assert.That(module.AfterGetCount, Is.EqualTo(1));
    }
}

[TrackedPropertyCollection]
public partial class TestClassPropertyCollection : ClassPropertyCollection
{
    [TrackedProperty]
    public partial int Value { get; set; }
}

public class CountModule : PropertyCollectionModule
{
    public int BeforeSetCount { get; private set; }
    public int AfterSetCount { get; private set; }
    public int BeforeGetCount { get; private set; }
    public int AfterGetCount { get; private set; }

    public override void BeforeSetValue(IPropertyCollection context, string name, object? value)
    {
        BeforeSetCount++;
        base.BeforeSetValue(context, name, value);
    }

    public override void AfterSetValue(IPropertyCollection context, string name, object? value)
    {
        AfterSetCount++;
        base.AfterSetValue(context, name, value);
    }

    public override void BeforeGetValue(IPropertyCollection context, string name)
    {
        BeforeGetCount++;
        base.BeforeGetValue(context, name);
    }

    public override void AfterGetValue(IPropertyCollection context, string name, object? value)
    {
        AfterGetCount++;
        base.AfterGetValue(context, name, value);
    }
}