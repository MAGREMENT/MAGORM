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

    public static readonly IEnumerable<ISelectFieldSpecification>? Star = null;
}

public record SelectFieldSpecification(string? Alias, bool IsTableColumn) : ISelectFieldSpecification;

public record SelectSpecification(string Model,
    IEnumerable<ISelectFieldSpecification>? Fields,
    WhereSpecification? Where = null,
    OrderBySpecification[]? OrderBy = null);
    
public record CreateFromSelectSpecification(string Name,
    SelectSpecification Select,
    bool IsTemporary);