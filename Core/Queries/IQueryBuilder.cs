using Core.Abstract;
using Core.Queries.Specifications;

namespace Core.Queries;

public interface IQueryBuilder
{
    Query Create(ModelSpecification specification);

    Query Insert(InsertSpecification specification);

    Query Update(UpdateSpecification specification);

    Query Select(SelectSpecification specification);

    bool IsSameDBFieldType(DBFieldType left, DBFieldType right);
}

public record FieldSpecification(string Name, DBFieldType FieldType, bool Unique, bool Required, bool AutoIncrement);

public record PrimaryKeySpecification(params IReadOnlyList<string> Names);

public record ForeignKeySpecification(string Field, string OtherModel, string OtherField);

public record ModelSpecification(string Model,
    IReadOnlyList<FieldSpecification> Fields,
    PrimaryKeySpecification PrimaryKey,
    IReadOnlyList<ForeignKeySpecification> ForeignKeys);
    
public record InsertSpecification(string Model,
    IReadOnlyList<string> Fields);

public record UpdateSpecification(string Model,
    IReadOnlyList<string> Fields,
    WhereSpecification? Where);

public enum OrderByType
{
    DESC, ASC
}

public record OrderBySpecification(string Field, OrderByType Type);

public record SelectSpecification(string Model,
    IReadOnlyList<string> Fields,
    WhereSpecification? Where,
    OrderBySpecification[] OrderBy);