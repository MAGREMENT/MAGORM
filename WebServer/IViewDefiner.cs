using API;

namespace WebServer;

public interface IViewDefiner
{
    void DefineView(View view);
}

public interface IViewProvider
{
    void DefineViews(IViewDefiner definer);
}

public class ViewEndpointDefiner(IEndpointDefiner _definer) : IViewDefiner
{
    public void DefineView(View view)
    {
        _definer.DefineEndpoint(new Endpoint(EndpointType.GET, view.Path, () 
            => new EndpointResult(EndpointResultType.Content, view.Html)));
    }
}