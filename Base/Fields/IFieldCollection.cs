namespace Base.Fields;

public interface IFieldCollection
{
    int GetFieldCount();
    
    IEnumerable<string> GetFieldsName();
    
    object? Get(string name);
    
    void Set(string name, object? value);
}