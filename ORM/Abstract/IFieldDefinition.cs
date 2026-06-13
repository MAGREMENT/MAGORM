using Base;

namespace ORM.Abstract;

public interface IFieldDefinition : INamed, IAttachable<Database>
{
    public FieldDefinitionsOptions Options { get; }

    public DBFieldType GetDBFieldType();
    
    public ModelReference? References { get; }
    
    //TODO divide into CheckValueValidity and ComputeValue
    public bool TryComputeValue<T>(object? value, T record, out object? result) where T : IRecord; 
}

public abstract class FieldDefinition : IFieldDefinition
{
    public abstract string Name { get; }

    public abstract void Attach(Database obj);

    public abstract void Detach(Database obj);

    public abstract FieldDefinitionsOptions Options { get; }

    public abstract DBFieldType GetDBFieldType();

    public abstract ModelReference? References { get; }

    public abstract bool TryComputeValue<T>(object? value, T record, out object? result) where T : IRecord;

    public override bool Equals(object? obj)
    {
        return obj is IFieldDefinition fd && fd.Name == Name;
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    public override string ToString()
    {
        return Name;
    }
}

public record ModelReference(string Model, string Field)
{
    public static ModelReference None = new(string.Empty, string.Empty);

    public bool IsNone() => Model.Length == 0;
}

public record FieldDefinitionsOptions(
    bool Required = false,
    bool Unique = false,
    bool AutoIncrement = false);

public enum DBFieldType
{
    NOT_DB_FIELD, INT, STRING, BOOL
}

public static class DBFieldTypeExtensions
{
    public static DBFieldType ToDBFieldType<T>()
    {
        var type = typeof(T);
        return ToDBFieldType(type);
    }

    public static DBFieldType ToDBFieldType(Type type)
    {
        if (type == typeof(int)) return DBFieldType.INT;
        if (type == typeof(string)) return DBFieldType.STRING;
        if (type == typeof(bool)) return DBFieldType.BOOL;

        return DBFieldType.NOT_DB_FIELD;
    }
}