using Base;
using ORM.ModelTypes;
using ORM.Queries;
using ORM.Queries.Specifications;

namespace ORM.Abstract;

public interface IFieldDefinition : INamed, IAttachable<IReadOnlyModelRegistry>, ISelectFieldSpecification
{
    public FieldDefinitionsOptions Options { get; }

    public DBFieldType GetDBFieldType();
    
    public ModelReference? Reference { get; }
    
    public bool TryFetchFromQueryResult(IQueryResult result, string name, out object? value); //TODO a bit stupid to have to give name here, try to find better ?

    public object? ToDbValue(object? recordValue);
}

public abstract class FieldDefinition : IFieldDefinition
{
    public abstract string Name { get; }

    public abstract void Attach(IReadOnlyModelRegistry obj);

    public abstract void Detach(IReadOnlyModelRegistry obj);

    public abstract FieldDefinitionsOptions Options { get; }

    public abstract DBFieldType GetDBFieldType();

    public abstract ModelReference? Reference { get; }

    public abstract bool TryFetchFromQueryResult(IQueryResult result, string name, out object? value);

    public abstract object? ToDbValue(object? recordValue);

    public string? Alias => null;

    public bool IsTableColumn => true;

    public object? Value => null;

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