namespace Base.PropertyCollections;

public abstract class PropertyCollectionModule : IPropertyCollectionModule
{
    public IPropertyCollectionModule? Next { get; set; }

    public virtual void BeforeSetValue(IPropertyCollection context, string name, object? value)
    {
        Next?.BeforeSetValue(context, name, value);
    }

    public virtual void AfterSetValue(IPropertyCollection context, string name, object? value)
    {
        Next?.AfterSetValue(context, name, value);
    }

    public virtual void BeforeGetValue(IPropertyCollection context, string name)
    {
        Next?.BeforeGetValue(context, name);
    }

    public virtual void AfterGetValue(IPropertyCollection context, string name, object? value)
    {
        Next?.AfterGetValue(context, name, value);
    }
}