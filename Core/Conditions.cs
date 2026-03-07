using Core.Queries.Specifications;

namespace Core;

public static class Conditions
{
    
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
            case string s :
                return new WhereArgument(s);
            default :
                parameters.Add(obj);
                return new WhereArgument();
        }
    }
}