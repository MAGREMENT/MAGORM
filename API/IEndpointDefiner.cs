namespace API;

public interface IEndpointDefiner
{
    void DefineEndpoint(Endpoint endpoint);

    void AddDefaultHandler(DefaultHandler handler);
}

public interface IEndpointProvider
{
    public void DefineEndpoints(IEndpointDefiner definer);
}

public delegate EndpointResult DefaultHandler(string path);

