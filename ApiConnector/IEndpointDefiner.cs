namespace ApiConnector;

public interface IEndpointDefiner
{
    void Define<T>(EndpointType type, string route, Dictionary<string, Type> schema, EndpointOperation<T> operation);
}

public delegate object? EndpointOperation<in T>(T parameters);

public enum EndpointType
{
    GET, POST, SET, DELETE
}