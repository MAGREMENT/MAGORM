using System.Text;
using ORM.Queries.Specifications;

namespace ORM.Queries;

public interface IQueryBuilder
{
    void Insert(InsertSpecification spec, IEnumerable<object>? parameters = null);
    
    void Create(CreateSpecification spec, IEnumerable<object>? parameters = null);

    void CreateFromSelect(CreateFromSelectSpecification spec, IEnumerable<object>? parameters = null);
    
    void Update(UpdateSpecification spec, IEnumerable<object>? parameters = null);
    
    void Select(SelectSpecification spec, IEnumerable<object>? parameters = null);

    void Drop(string name);

    Query ToQuery();

    Query[] ToQueries();

    void Reset();
}

public class StackingQueryBuilder : IQueryBuilder
{
    private readonly StringBuilder _builder = new();
    private int _paramCount;
    private readonly List<object> _parameters = new();
    private readonly ISqlLanguage _language;

    public StackingQueryBuilder(ISqlLanguage language)
    {
        _language = language;
    }

    public void Insert(InsertSpecification spec, IEnumerable<object>? parameters = null)
    {
        BeforeLanguageCall(parameters);
        _language.Insert(_builder, ref _paramCount, spec);
    }

    public void Create(CreateSpecification spec, IEnumerable<object>? parameters = null)
    {
        BeforeLanguageCall(parameters);
        _language.Create(_builder, ref _paramCount, spec);
    }

    public void CreateFromSelect(CreateFromSelectSpecification spec, IEnumerable<object>? parameters = null)
    {
        BeforeLanguageCall(parameters);
        _language.CreateFromSelect(_builder, ref _paramCount, spec);
    }

    public void Update(UpdateSpecification spec, IEnumerable<object>? parameters = null)
    {
        BeforeLanguageCall(parameters);
        _language.Update(_builder, ref _paramCount, spec);
    }

    public void Select(SelectSpecification spec, IEnumerable<object>? parameters = null)
    {
        BeforeLanguageCall(parameters);
        _language.Select(_builder, ref _paramCount, spec);
    }

    public void Drop(string name)
    {
        BeforeLanguageCall(null);
        _language.Drop(_builder, ref _paramCount, name);
    }

    public Query ToQuery()
    {
        if (_parameters.Count != _paramCount) throw new Exception(); //TODO

        return new Query(_builder.ToString(), _parameters.ToArray());
    }

    public Query[] ToQueries() => [ToQuery()];

    public void Reset()
    {
        _builder.Clear();
        _parameters.Clear();
        _paramCount = 0;
    }
    
    private void BeforeLanguageCall(IEnumerable<object>? parameters)
    {
        if (parameters is not null) _parameters.AddRange(parameters);
        if (_builder.Length > 0) _builder.Append(";\n\n");
    }
}

public class DividedQueriesBuilder : IQueryBuilder
{
    private readonly List<Query> _queries = [];
    private readonly StringBuilder _builder = new();
    private readonly ISqlLanguage _language;

    public DividedQueriesBuilder(ISqlLanguage language)
    {
        _language = language;
    }

    public void Insert(InsertSpecification spec, IEnumerable<object>? parameters = null)
    {
        var paramCount = 0;
        _language.Insert(_builder, ref paramCount, spec);
        AfterLanguageCall(paramCount, parameters);
    }

    public void Create(CreateSpecification spec, IEnumerable<object>? parameters = null)
    {
        var paramCount = 0;
        _language.Create(_builder, ref paramCount, spec);
        AfterLanguageCall(paramCount, parameters);
    }

    public void CreateFromSelect(CreateFromSelectSpecification spec, IEnumerable<object>? parameters = null)
    {
        var paramCount = 0;
        _language.CreateFromSelect(_builder, ref paramCount, spec);
        AfterLanguageCall(paramCount, parameters);
    }

    public void Update(UpdateSpecification spec, IEnumerable<object>? parameters = null)
    {
        var paramCount = 0;
        _language.Update(_builder, ref paramCount, spec);
        AfterLanguageCall(paramCount, parameters);
    }

    public void Select(SelectSpecification spec, IEnumerable<object>? parameters = null)
    {
        var paramCount = 0;
        _language.Select(_builder, ref paramCount, spec);
        AfterLanguageCall(paramCount, parameters);
    }
    
    public void Drop(string name)
    {
        var paramCount = 0;
        _language.Drop(_builder, ref paramCount, name);
        AfterLanguageCall(paramCount, null);
    }

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
    }

    private void AfterLanguageCall(int paramCount, IEnumerable<object>? parameters)
    {
        var paramArray = parameters is null ? [] : parameters.ToArray();
        if (paramArray.Length != paramCount) throw new Exception(); //TODO

        var query = new Query(_builder.ToString(), paramArray);
        _builder.Clear();
        _queries.Add(query);
    }
}