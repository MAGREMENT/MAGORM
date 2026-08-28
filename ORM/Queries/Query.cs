namespace ORM.Queries;

public record Query(string String, IReadOnlyList<object?> Parameters);