namespace Base.Fields.Implementations;

[PropertyFieldCollection]
public abstract partial class PropertyFieldCollection : IFieldCollection
{
    public abstract bool TryGet(string key, out object? value);

    public object? Get(string key) => KeyValue.DefaultGet(this, key);

    public abstract void Set(string key, object? value);

    public abstract int GetFieldCount();

    public abstract IEnumerable<string> GetFieldsName();
}

[AttributeUsage(AttributeTargets.Class)]
public class PropertyFieldCollectionAttribute : Attribute;

[AttributeUsage(AttributeTargets.Property)]
public class FieldAttribute : Attribute;