using Base;
using ORM.ModelTypes;
using ORM.Queries.Specifications;

namespace ORM.Abstract;

public interface IFieldDefinition : INamed, IAttachable<IReadOnlyModelBank>, ISelectFieldSpecification
{
    public FieldDefinitionsOptions Options { get; }

    public DBFieldType GetDBFieldType();
    
    public ModelReference? Reference { get; }
    
    //TODO divide into CheckValueValidity and ComputeValue
    public bool TryComputeValue<T>(object? value, T record, out object? result) where T : IRecord;

    public object? ToDbValue(object? recordValue);
}

public abstract class FieldDefinition : IFieldDefinition
{
    public abstract string Name { get; }

    public abstract void Attach(IReadOnlyModelBank obj);

    public abstract void Detach(IReadOnlyModelBank obj);

    public abstract FieldDefinitionsOptions Options { get; }

    public abstract DBFieldType GetDBFieldType();

    public abstract ModelReference? Reference { get; }

    public abstract bool TryComputeValue<T>(object? value, T record, out object? result) where T : IRecord;
    
    public abstract object? ToDbValue(object? recordValue);

    public string? Alias => null;

    public bool IsTableColumn => true;

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

public record ModelReference(IModel Model, IFieldDefinition Field);

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