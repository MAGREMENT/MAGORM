using ORM.Abstract;
using ORM.Queries.Builder;
using ORM.Queries.Specifications;

namespace ORM.Queries;

public interface ISqlLanguage
{
    IScriptBuilder InitScriptBuilder();
    
    void Create(IScriptBuilder builder, CreateSpecification specification);
    
    void CreateFromSelect(IScriptBuilder builder, CreateFromSelectSpecification specification);

    void Insert(IScriptBuilder builder, InsertSpecification specification);

    void Update(IScriptBuilder builder, UpdateSpecification specification);

    void Select(IScriptBuilder builder, SelectSpecification specification);

    void Delete(IScriptBuilder builder, DeleteSpecification specification);

    void Drop(IScriptBuilder builder, string name);

    bool Nuke(IScriptBuilder builder);

    bool IsSameDBFieldType(DBFieldType left, DBFieldType right);
}
    
public record InsertSpecification(string Model,
    IReadOnlyList<string> Fields,
    IReadOnlyList<object?> Values,
    IReadOnlyList<string> ReturnedFields);

public record UpdateSpecification(string Model,
    IReadOnlyList<string> Fields,
    IReadOnlyList<object?> Values,
    WhereSpecification? Where = null);
    
public record DeleteSpecification(string Model,
    WhereSpecification? Where = null);