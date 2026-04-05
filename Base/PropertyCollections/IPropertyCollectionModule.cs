namespace Base.PropertyCollections;

public interface IPropertyCollectionModule
{
    internal IPropertyCollectionModule? Next { get; set; }
    
    void BeforeSetValue(IPropertyCollection context, string name, object? value);

    void AfterSetValue(IPropertyCollection context, string name, object? value);

    void BeforeGetValue(IPropertyCollection context, string name);

    void AfterGetValue(IPropertyCollection context, string name, object? value);
}