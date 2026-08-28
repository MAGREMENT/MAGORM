using System.Collections;
using System.Text;

namespace ORM.Queries.Builder;

public class StackingScriptBuilder : IScriptBuilder
{
    private readonly StringBuilder _builder = new();
    private readonly List<object?> _parameters = new();
    
    public IScriptBuilder New()
    {
        return new StackingScriptBuilder();
    }

    public void EndStatement()
    {
        _builder.Append(";\n\n");
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

    public Query ToQuery()
    {
        return new Query(_builder.ToString(), _parameters.ToArray());
    }

    public Query[] ToQueries() => [ToQuery()];

    public void Reset()
    {
        _builder.Clear();
        _parameters.Clear();
    }

    public IEnumerator<Query> GetEnumerator()
    {
        yield return new Query(_builder.ToString(), _parameters);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => 1;

    public Query this[int index] => index == 0 
        ? new Query(_builder.ToString(), _parameters) 
        : throw new ArgumentException();
}