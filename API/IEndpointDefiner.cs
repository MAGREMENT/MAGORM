using Base.Fields;

namespace API;

public interface IEndpointDefiner
{
    void Define(Endpoint endpoint, IEnumerable<EndpointParameter> parameters, EndpointOperation operation);

    void Define<T>(Endpoint endpoint, T operation) where T : Delegate;

    void AddDefaultHandler(DefaultHandler handler);
}

public delegate object? EndpointOperation(IReadOnlyKeyValue<string, object?> values);

public delegate object? DefaultHandler(string path);

