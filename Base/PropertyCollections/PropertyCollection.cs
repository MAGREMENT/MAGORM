namespace Base.PropertyCollections;

public abstract class PropertyCollection : IPropertyCollection
{
    private IPropertyCollectionModule? _module;
    
    public abstract int GetPropertyCount();

    public abstract IEnumerable<string> GetPropertiesName();

    public void SetValue(string name, object? value)
    {
        BeforeSetValue(this, name, value);
        InternalSetValue(name, value);
        AfterSetValue(this, name, value);
    }

    public object? GetValue(string name)
    {
        BeforeGetValue(this, name);
        var value = InternalGetValue(name);
        AfterGetValue(this, name, value);
        return value;
    }

    public void InsertModule(IPropertyCollectionModule module)
    {
        module.Next = _module;
        _module = module;
    }

    public void AppendModule(IPropertyCollectionModule module)
    {
        if (_module is null)
        {
            _module = module;
            return;
        }

        var curr = _module;
        while (curr.Next is not null) curr = curr.Next;

        curr.Next = module;
    }

    protected abstract void InternalSetValue(string name, object? value);

    protected abstract object? InternalGetValue(string name);

    protected void BeforeSetValue(IPropertyCollection context, string name, object? value)
    {
        _module?.BeforeSetValue(context, name, value);
    }

    protected void AfterSetValue(IPropertyCollection context, string name, object? value)
    {
        _module?.AfterSetValue(context, name, value);
    }

    protected void BeforeGetValue(IPropertyCollection context, string name)
    {
        _module?.BeforeGetValue(context, name);
    }

    protected void AfterGetValue(IPropertyCollection context, string name, object? value)
    {
        _module?.AfterGetValue(context, name, value);
    }
}