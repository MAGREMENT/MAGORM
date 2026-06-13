namespace ORM.Queries.Specifications;

public enum OrderByType
{
    DESC, ASC
}

public record OrderBySpecification(string Field, OrderByType Type);

public record TemporaryTableSpecification(string Name, bool IsSelected);

public record SelectSpecification(string Model,
    IEnumerable<string> Fields,
    WhereSpecification? Where = null,
    OrderBySpecification[]? OrderBy = null);
    
public record CreateFromSelectSpecification(string Name,
    SelectSpecification Select,
    bool IsTemporary);