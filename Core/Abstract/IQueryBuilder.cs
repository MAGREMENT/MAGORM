using System.Text;

namespace Core.Abstract;

public interface IQueryBuilder
{
    Query Create(ModelSpecification specification);

    Query Insert(InsertSpecification specification);

    Query Update(UpdateSpecification specification);

    Query Select(SelectSpecification specification);

    bool IsSameDBFieldType(DBFieldType left, DBFieldType right);
}

public record Query(string String, int ParameterCount)
{
    public void CheckParameters(object[] parameters)
    {
        if (ParameterCount != parameters.Length) throw new ArgumentException("Wrong parameter count");
    }
}

public class QueryStacker
{
    private readonly StringBuilder _string = new();
    private int _paramCount = 0;
    private readonly List<object> _parameters = new();

    public void AddQuery(Query query)
    {
        _string.Append(query.String);
        _string.Append("\n\n");
        _paramCount += query.ParameterCount;
    }

    public void AddParameter(object p) => _parameters.Add(p);

    public void AddParameters(object[] parameters) => _parameters.AddRange(parameters);

    public Query GetQuery() => new Query(_string.ToString(), _paramCount);

    public object[] GetParameters() => _parameters.ToArray();
}

public record FieldSpecification(string Name, DBFieldType FieldType, bool Unique, bool Required, bool AutoIncrement);

public record PrimaryKeySpecification(params string[] Names);

public record ForeignKeySpecification(string Field, string OtherModel, string OtherField);

public record ModelSpecification(string Model,
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