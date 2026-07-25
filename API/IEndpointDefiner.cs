using Base.Fields;

namespace API;

public interface IEndpointDefiner
{
    void Define(Endpoint endpoint, EndpointOperation operation);
}

public delegate object? EndpointOperation(IReadOnlyKeyValue<string, object?> values);

