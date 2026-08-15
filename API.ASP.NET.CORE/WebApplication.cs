using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Primitives;

namespace API.ASP.NET.CORE;

public class WebApplicationApi : IApi
{
    private readonly WebApplication _app;
    private readonly DynamicEndpointDataSource _dynamic = new();
    private bool _started;

    public WebApplicationApi(WebApplication app)
    {
        _app = app;
        ((IEndpointRouteBuilder)_app).DataSources.Add(_dynamic);
    }
    
    public void DefineEndpoint(Endpoint endpoint)
    {
        if (_started)
        {
            var builder = new RouteEndpointBuilder(
                RequestDelegateFactory.Create(endpoint.Operation).RequestDelegate,
                RoutePatternFactory.Parse(endpoint.Route),
                0);
        
            builder.FilterFactories.Insert(builder.FilterFactories.Count, (_, next) => context => DictionaryJsonToObjectFiler(context, next));
            builder.FilterFactories.Insert(builder.FilterFactories.Count, (_, next) => context => HandleEndpointResultFilter(context, next));
            _dynamic.Add([(RouteEndpoint)builder.Build()]);
        }

        _app.MapMethods(endpoint.Route, [endpoint.Type.ToString()], endpoint.Operation)
            .AddEndpointFilter(DictionaryJsonToObjectFiler)
            .AddEndpointFilter(HandleEndpointResultFilter);
    }

    public void AddDefaultHandler(DefaultHandler handler)
    {
        _app.Use(async (context, next) =>
        {
            await next(context);

            if (context.Response is { StatusCode: 404, HasStarted: false })
            {
                var result = ToIResult(handler(context.Request.Path));
                context.Response.StatusCode = 200;
                await result.ExecuteAsync(context);
            }
        });
    }

    public void Start()
    {
        _started = true;
        _app.RunAsync();
    }

    private static async ValueTask<object?> DictionaryJsonToObjectFiler(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        foreach(var arg in context.Arguments)
        {
            if(arg is Dictionary<string, object?> dic) ConvertDictionaryJsonToObject(dic);
            else if (arg is Dictionary<string, object?>[] dicArray)
            {
                foreach (var d in dicArray)
                {
                    ConvertDictionaryJsonToObject(d);
                }
            }
        }
        
        return await next(context);
    }

    private static void ConvertDictionaryJsonToObject(Dictionary<string, object?> dic)
    {
        foreach (var entry in dic)
        {
            if(entry.Value is null) continue;
            dic[entry.Key] = ToObject((JsonElement)entry.Value);
        }
    }
    
    private static object? ToObject(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),

            JsonValueKind.Number =>
                element.TryGetInt32(out var i) ? i :
                element.TryGetInt64(out var l) ? l :
                element.TryGetDecimal(out var d) ? d :
                element.GetDouble(),

            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,

            JsonValueKind.Array =>
                element.EnumerateArray().Select(ToObject).ToArray(),

            JsonValueKind.Object =>
                element.EnumerateObject()
                    .ToDictionary(p => p.Name, p => ToObject(p.Value)),

            _ => null
        };
    }

    private static async ValueTask<object?> HandleEndpointResultFilter(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var result = await next(context);
        if (result is not EndpointResult er) throw new Exception("Should return an endpoint result");
        
        return ToIResult(er);
    }

    private static IResult ToIResult(EndpointResult er)
    {
        return er.Type switch
        {
            EndpointResultType.File => Results.File((string)er.Value,
                ApiHelper.ExtensionToContentType(Path.GetExtension((string)er.Value))),
            EndpointResultType.Json => Results.Json(er.Value),
            EndpointResultType.NotFound => Results.NotFound(),
            EndpointResultType.BadRequest => Results.BadRequest(er.Value),
            EndpointResultType.Content => Results.Content((string)er.Value, "text/html") //TODO
        };
    }
}

//TODO Does not work ??? https://www.mariusgundersen.net/article/2021-dynamic-endpoint-routing/
public class DynamicEndpointDataSource : EndpointDataSource
{
    private readonly Lock _lock = new Lock();
    private readonly List<RouteEndpoint> _endpoints = new();

    private CancellationTokenSource? _cancellationTokenSource;

    private IChangeToken? _changeToken;


    public override IReadOnlyList<RouteEndpoint> Endpoints
    {
        get
        {
            RouteEndpoint[] result;
            lock (_lock)
            {
                result = _endpoints.ToArray();
            }

            return result;
        }
    }

    public override IChangeToken GetChangeToken()
        => _changeToken!;

    public void Add(RouteEndpoint[] endpoints)
    {
        lock (_lock)
        {
            _endpoints.AddRange(endpoints);
        }
        
        NotifyChanged();
    }

    private void NotifyChanged()
    {
        var oldCancellationTokenSource = _cancellationTokenSource;
        
        _cancellationTokenSource = new CancellationTokenSource();
        _changeToken = new CancellationChangeToken(_cancellationTokenSource.Token);

        oldCancellationTokenSource?.Cancel();
    }
}