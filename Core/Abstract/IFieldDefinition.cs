using Base;

namespace Core.Abstract;

public interface IFieldDefinition : INamed, IAttachable<Model>
{
    public FieldDefinitionsOptions Options { get; }

    public DBFieldType GetDBFieldType();
    
    public ModelReference[] References { get; }
    
    //TODO divide into CheckValueValidity and ComputeValue
    public bool TryComputeValue<T>(object? value, T record, out object? result) where T : IRecord; 
}

public record ModelReference(string Model, string Field);

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