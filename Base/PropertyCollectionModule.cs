namespace Base;

public abstract class PropertyCollectionModule(IPropertyCollectionModule? _next = null) : IPropertyCollectionModule
{
    public virtual void BeforeSetValue(string name, object value)
    {
        _next?.BeforeSetValue(name, value);
    }

    public virtual void AfterSetValue(string name, object value)
    {
        _next?.AfterSetValue(name, value);
    }

    public virtual void BeforeGetValue(string name)
    {
        _next?.BeforeGetValue(name);
    }

    public virtual void AfterGetValue(string name, object value)
    {
        _next?.AfterGetValue(name, value);
    }
}