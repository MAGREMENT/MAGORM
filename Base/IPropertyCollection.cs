namespace Base;

public interface IPropertyCollection
{
    int GetPropertyCount();
    
    IEnumerable<string> GetPropertiesName();
    
    void SetValue(string name, object? value);
    
    object? GetValue(string name);
}