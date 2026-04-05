namespace Base.PropertyCollections;

public interface IPropertyCollection
{
    int GetPropertyCount();
    
    IEnumerable<string> GetPropertiesName();
    
    void SetValue(string name, object? value);
    
    object? GetValue(string name);

    void InsertModule(IPropertyCollectionModule module);

    void AppendModule(IPropertyCollectionModule module);
}