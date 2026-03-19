namespace Base;

public abstract class PropertyCollection(IPropertyCollectionModule? _module = null) : IPropertyCollection
{
    public abstract int GetPropertyCount();

    public abstract IEnumerable<string> GetPropertiesName();

    public void SetValue(string name, object value)
    {
        BeforeSetValue(name, value);
        InternalSetValue(name, value);
        AfterSetValue(name, value);
    }

    public object GetValue(string name)
    {
        BeforeGetValue(name);
        var value = InternalGetValue(name);
        AfterGetValue(name, value);
        return value;
    }

    protected abstract void InternalSetValue(string name, object value);

    protected abstract object InternalGetValue(string name);

    protected void BeforeSetValue(string name, object value)
    {
        _module?.BeforeSetValue(name, value);
    }

    protected void AfterSetValue(string name, object value)
    {
        _module?.AfterSetValue(name, value);
    }

    protected void BeforeGetValue(string name)
    {
        _module?.BeforeGetValue(name);
    }

    protected void AfterGetValue(string name, object value)
    {
        _module?.AfterGetValue(name, value);
    }
}