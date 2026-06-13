using ORM.Queries;
using ORM.Queries.Specifications;

namespace ORM;

public static class Conditions
{
    public static QueryCondition And(IReadOnlyList<QueryCondition> conditions)
        => RepeatInline(conditions, DBOperator.AND);

    public static QueryCondition Or(IReadOnlyList<QueryCondition> conditions)
        => RepeatInline(conditions, DBOperator.OR);

    private static QueryCondition RepeatInline(IReadOnlyList<QueryCondition> conditions, DBOperator op)
    {
        //TODO handle Count == 0
        if (conditions.Count == 1) return conditions[0];

        var curr = conditions[0];
        for (int i = 1; i < conditions.Count; i++)
        {
            curr = new QueryCondition(curr, op, conditions[i]);
        }

        return curr;
    }
}

public record QueryCondition(object Left, DBOperator Operator, object Right)
{
    public (WhereSpecification, IReadOnlyList<object>) Compile()
    {
        var parameters = new List<object>();
        var spec = Compile(parameters);
        return (spec, parameters);
    }

    private WhereSpecification Compile(List<object> parameters)
    {
        return new WhereSpecification(GetArgument(Left, parameters), Operator, GetArgument(Right, parameters));
    }

    private WhereArgument GetArgument(object obj, List<object> parameters)
    {
        switch (obj)
        {
            case QueryCondition qc :
                return new WhereArgument(qc.Compile(parameters));
            case Query query :
                parameters.AddRange(query.Parameters);
                return new WhereArgument('(' + query.String + ')');
            case string s :
                return new WhereArgument(s);
            default :
                parameters.Add(obj);
                return new WhereArgument();
        }
    }
}