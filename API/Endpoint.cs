namespace API;

public enum EndpointType
{
    GET, POST, PUT, DELETE
}

public delegate bool CheckParameterValue(object? value);

public record EndpointParameterCheck(string Name, Type Type, bool Optional = false, CheckParameterValue? Check = null); //TODO implement in .NET API

public record Endpoint(EndpointType Type, string Route, Delegate Operation, IEnumerable<EndpointParameterCheck>? Checks = null);

public enum EndpointResultType
{
    File, Json, Content, NotFound, BadRequest
}

public record EndpointResult(EndpointResultType Type, object? Value = null)
{
    public static EndpointResult Content(string s) => new(EndpointResultType.Content, s);
    //TODO others
}