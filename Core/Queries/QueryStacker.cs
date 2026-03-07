using System.Text;

namespace Core.Queries;

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