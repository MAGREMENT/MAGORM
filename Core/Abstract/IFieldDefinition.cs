using System.Diagnostics.CodeAnalysis;

namespace Core.Abstract;

public interface IFieldDefinition : IDependencies
{
    public FieldDefinitionsOptions Options { get; }

    public DBFieldType GetFieldType();

    public bool IsStored();
    
    public bool TryFetchValue<T>(string? value, T record, [NotNullWhen(true)] out object? result) where T : IRecord; 
}

public record FieldDefinitionsOptions(bool Required = false, 
    bool Unique = false, 
    bool AutoIncrement = false, 
    OnChange<IRecord>? OnChange = null);

public delegate void OnChange<in T>(object oldValue, object newValue, T record, AbstractDatabase db) where T : IRecord;

public enum DBFieldType
{
    INT, VARCHAR, BOOL
}

public static class DBFieldTypeExtensions
{
    public static DBFieldType ToDBFieldType<T>()
    {
        var type = typeof(T);
        if (type == typeof(int)) return DBFieldType.INT;
        if (type == typeof(string)) return DBFieldType.VARCHAR;
        if (type == typeof(bool)) return DBFieldType.BOOL;

        throw new ArgumentOutOfRangeException();
    }
}