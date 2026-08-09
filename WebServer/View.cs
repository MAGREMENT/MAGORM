using API;

namespace WebServer;

public static class View
{
    public static EndpointResult Render(string content)
    {
        return EndpointResult.Content(content);
    }

    public static EndpointResult Render(string wrapper, string[] contents)
    {
        return EndpointResult.Content(wrapper); //TODO
    }
}