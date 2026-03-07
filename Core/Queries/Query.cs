namespace Core.Queries;

public record Query(string String, int ParameterCount)
{
    public void CheckParameters(IReadOnlyList<object> parameters)
    {
        if (ParameterCount != parameters.Count) throw new ArgumentException("Wrong parameter count");
    }
}