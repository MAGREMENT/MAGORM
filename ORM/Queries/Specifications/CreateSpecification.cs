using ORM.Abstract;

namespace ORM.Queries.Specifications;

public record FieldSpecification(string Name, DBFieldType FieldType, bool Unique, bool Required, bool AutoIncrement);

public record PrimaryKeySpecification(params IReadOnlyList<string> Names);

public record ForeignKeySpecification(string Field, string OtherModel, string OtherField);

public record CreateSpecification(string Model,
    IReadOnlyList<FieldSpecification> Fields,
    PrimaryKeySpecification PrimaryKey,
    IReadOnlyList<ForeignKeySpecification> ForeignKeys);