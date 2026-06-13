using System.Text;
using ORM.Abstract;
using ORM.Queries.Specifications;

namespace ORM.Queries;

public interface ISqlLanguage
{
    void Create(StringBuilder builder, ref int paramCount, CreateSpecification specification);
    
    void CreateFromSelect(StringBuilder builder, ref int paramCount, CreateFromSelectSpecification specification);

    void Insert(StringBuilder builder, ref int paramCount, InsertSpecification specification);

    void Update(StringBuilder builder, ref int paramCount, UpdateSpecification specification);

    void Select(StringBuilder builder, ref int paramCount, SelectSpecification specification);

    void Drop(StringBuilder builder, ref int paramCount, string name);

    bool IsSameDBFieldType(DBFieldType left, DBFieldType right);

    IQueryBuilder InitQueryBuilder();
}
    
public record InsertSpecification(string Model,
    IReadOnlyList<string> Fields,
    IReadOnlyList<string> ReturnedFields);

public record UpdateSpecification(string Model,
    IReadOnlyList<string> Fields,
    WhereSpecification? Where);