namespace APIConnector;

public interface IEndpointDefiner
{
    void Define<T>(EndpointSpecification<T> spec);
}

public delegate void DefineEndpoint<T>(EndpointSpecification<T> spec);

public record EndpointSpecification<T>(
    EndpointType Type,
    string Route,
    EndpointOperation<T> Operation);

public delegate object? EndpointOperation<in T>(T parameters);

public enum EndpointType
{
    GET, POST, PUT, DELETE
}