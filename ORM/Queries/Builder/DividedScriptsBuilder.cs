using System.Collections;
using System.Text;

namespace ORM.Queries.Builder;

public class DividedScriptsBuilder : IScriptBuilder
{
    private readonly List<Query> _queries = [];
    private readonly StringBuilder _builder = new();
    private readonly List<object?> _parameters = new();
    
    public IScriptBuilder New()
    {
        return new DividedScriptsBuilder();
    }

    public void EndStatement()
    {
        var query = new Query(_builder.ToString(), _parameters.ToArray());
        _builder.Clear();
        _parameters.Clear();
        _queries.Add(query);
    }

    public void Append(string s)
    {
        _builder.Append(s);
    }

    public void Append(char c)
    {
        _builder.Append(c);
    }

    public void AppendParameter(object? v)
    {
        _builder.Append('@');
        _builder.Append(_parameters.Count);
        _parameters.Add(v);
    }

    public int CurrentParameterCount { get; set; }

    public Query ToQuery()
    {
        if (_queries.Count > 1) throw new Exception();

        return _queries[0];
    }

    public Query[] ToQueries() => _queries.ToArray();

    public void Reset()
    {
        _queries.Clear();
        _builder.Clear();
        _parameters.Clear();
    }

    public IEnumerator<Query> GetEnumerator()
    {
        return _queries.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => _queries.Count;

    public Query this[int index] => _queries[index];
}