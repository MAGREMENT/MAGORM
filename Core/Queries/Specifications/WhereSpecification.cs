using System.Runtime.InteropServices;

namespace Core.Queries.Specifications;

public record WhereSpecification(WhereArgument Left, DBOperator Operator, WhereArgument Right);

public readonly struct WhereArgument
{
    public readonly WhereArgumentType Type;
    
    public readonly string Text;
    
    public readonly WhereSpecification Argument;

    public WhereArgument(string text)
    {
        Type = WhereArgumentType.FIELD;
        Text = text;
        Argument = null!;
    }
    
    public WhereArgument(WhereSpecification argument)
    {
        Type = WhereArgumentType.ARGUMENT;
        Text = null!;
        Argument = argument;
    }

    public WhereArgument()
    {
        Type = WhereArgumentType.PARAMETER;
        Text = null!;
        Argument = null!;
    }
}

public enum WhereArgumentType
{
    FIELD, ARGUMENT, PARAMETER
}

public enum DBOperator
{
    NONE = -1, PLUS, MINUS, OR, AND, EQUAL, LIKE, IS
}