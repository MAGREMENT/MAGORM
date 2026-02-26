using System.Diagnostics.CodeAnalysis;

namespace Core.Abstract;

public interface IFieldDefinition : IDependencies, IDatabaseAttachable
{
    public FieldDefinitionsOptions Options { get; }

    public DBFieldType GetDBFieldType();

    public bool IsStored();
    
    public ModelReference[] References { get; }
    
    public bool TryComputeValue<T>(object? value, T record, [NotNullWhen(true)] out object? result) where T : IRecord; 
}

public record ModelReference(string Model, string Field);

public record FieldDefinitionsOptions(bool Required = false, 
    bool Unique = false, 
    bool AutoIncrement = false);

public enum DBFieldType
{
    INT, STRING, BOOL
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

        throw new ArgumentOutOfRangeException();
    }
}