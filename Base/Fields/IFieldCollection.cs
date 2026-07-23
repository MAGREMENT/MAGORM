namespace Base.Fields;

public interface IFieldCollection : IKeyValue<string, object?>
{
    int GetFieldCount();
    
    IEnumerable<string> GetFieldsName();
}