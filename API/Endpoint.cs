namespace API;

public enum EndpointType
{
    GET, POST, PUT, DELETE
}

public delegate bool CheckParameterValue(object? value);

public record EndpointParameter(string Name, Type Type, bool Optional = false, CheckParameterValue? Check = null);

public record Endpoint(EndpointType Type, string Route);