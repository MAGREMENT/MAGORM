using Bridge.ORM.APIConnector;
using Microsoft.AspNetCore.Builder;
using ORM.Abstract;

namespace APIConnector.ASP.NET.CORE;

public static class WebApplicationExtensions
{
    public static void MapModels(this WebApplication app, Database database)
    {
        var definer = new WebApplicationEndpointDefiner(app);
        foreach (var model in database.EnumerateModels())
        {
            if(model is not IApiModel em) continue;
            em.DefineEndpoints(definer);
        }
    }
}

public class WebApplicationEndpointDefiner(WebApplication _app) : IEndpointDefiner
{
    public void Define<T>(EndpointSpecification<T> spec)
    {
        switch (spec.Type)
        {
            case EndpointType.GET : _app.MapGet(spec.Route, spec.Operation);
                break;
            case EndpointType.POST : _app.MapPost(spec.Route, spec.Operation);
                break;
            case EndpointType.PUT : _app.MapPut(spec.Route, spec.Operation);
                break;
            case EndpointType.DELETE : _app.MapDelete(spec.Route, spec.Operation);
                break;
        }
    }
}