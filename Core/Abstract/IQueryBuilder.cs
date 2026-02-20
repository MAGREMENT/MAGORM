namespace Core.Abstract;

public interface IQueryBuilder
{
    Query Create(CreateSpecification specification);

    Query Insert(InsertSpecification specification);

    Query Update(UpdateSpecification specification);

    Query Select(SelectSpecification specification);
}

public record Query(string String, int ParameterCount);

public record FieldSpecification(string Name, DBFieldType FieldType, bool Unique, bool Required, bool AutoIncrement);

public record PrimaryKeySpecification(params string[] Names);

public record ForeignKeySpecification(string Field, string OtherModel, string OtherField);

public record CreateSpecification(string Model,
    IReadOnlyList<FieldSpecification> Fields,
    PrimaryKeySpecification PrimaryKey,
    IReadOnlyList<ForeignKeySpecification> ForeignKeys);
    
public record InsertSpecification(string Model,
    string[] Fields);
    
public record WhereSpecification();

public record UpdateSpecification(string Model,
    string[] Fields,
    WhereSpecification Where);

public enum OrderByType
{
    DESC, ASC
}

public record OrderBySpecification(string Field, OrderByType Type);

public record SelectSpecification(string Model,
    string[] Fields,
    WhereSpecification Where,
    OrderBySpecification[] OrderBy);