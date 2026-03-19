using Base;
using BaseGenerator;

namespace Tests;

public class PropertyCollectionGeneratorTests
{
    [Test]
    public void Test()
    {
        var cls = new TestClassPropertyCollection();
        var a = 1;
    }
}

public partial class TestClassPropertyCollection : ClassPropertyCollection
{
    [GeneratedProperty]
    public partial int Value { get; set; }
}

[GeneratedPropertyCollection]
public class AAAA
{
    
}

public class CountModule : PropertyCollectionModule
{
    public int BeforeSetCount { get; private set; }
    public int AfterSetCount { get; private set; }
    public int BeforeGetCount { get; private set; }
    public int AfterGetCount { get; private set; }

    public override void BeforeSetValue(string name, object value)
    {
        BeforeSetCount++;
        base.BeforeSetValue(name, value);
    }

    public override void AfterSetValue(string name, object value)
    {
        AfterSetCount++;
        base.AfterSetValue(name, value);
    }

    public override void BeforeGetValue(string name)
    {
        BeforeGetCount++;
        base.BeforeGetValue(name);
    }

    public override void AfterGetValue(string name, object value)
    {
        AfterGetCount++;
        base.AfterGetValue(name, value);
    }
}