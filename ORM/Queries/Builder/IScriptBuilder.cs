namespace ORM.Queries.Builder;

public interface IScriptBuilder : IReadOnlyList<Query>
{
    Query ToQuery();

    Query[] ToQueries();

    void Reset();


    IScriptBuilder New();

    
    void EndStatement();


    void Append(string s);
    
    void Append(char c);
    void AppendParameter(object? v);
}