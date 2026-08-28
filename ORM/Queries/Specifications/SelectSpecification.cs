namespace ORM.Queries.Specifications;

public enum OrderByType
{
    DESC, ASC
}

public record OrderBySpecification(string Field, OrderByType Type);

public interface ISelectFieldSpecification
{
    string? Alias { get; }
    bool IsTableColumn { get; }
    object? Value { get; }

    public static readonly IEnumerable<ISelectFieldSpecification>? Star = null;
}

public record SelectFieldSpecification : ISelectFieldSpecification
{
    public SelectFieldSpecification(string? alias)
    {
        Alias = alias;
        IsTableColumn = true;
        Value = null;
    }

    public SelectFieldSpecification(object? value, string? alias)
    {
        Alias = alias;
        IsTableColumn = false;
        Value = value;
    }

    public string? Alias { get; }
    public bool IsTableColumn { get; }
    public object? Value { get; }
}

public record SelectSpecification(string Model,
    IEnumerable<ISelectFieldSpecification>? Fields,
    WhereSpecification? Where = null,
    OrderBySpecification[]? OrderBy = null);
    
public record CreateFromSelectSpecification(string Name,
    SelectSpecification Select,
    bool IsTemporary);