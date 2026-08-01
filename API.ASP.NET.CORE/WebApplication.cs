using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace API.ASP.NET.CORE;

public class WebApplicationEndpointDefiner(WebApplication _app) : IEndpointDefiner
{
    public void DefineEndpoint(Endpoint endpoint)
    {
        var builder = _app.MapMethods(endpoint.Route, [endpoint.Type.ToString()], endpoint.Operation)
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
                GetContentType(Path.GetExtension((string)er.Value))),
            EndpointResultType.Json => Results.Json(er.Value),
            EndpointResultType.NotFound => Results.NotFound(),
            EndpointResultType.BadRequest => Results.BadRequest(er.Value),
            EndpointResultType.Content => Results.Content((string)er.Value, "text/html") //TODO
        };
    }

    private static string GetContentType(string extension)
    {
        return extension switch
        {
            ".html" or ".htm" => "text/html",
            ".css"            => "text/css",
            ".js"             => "application/javascript",
            ".json"           => "application/json",
            ".xml"            => "application/xml",
            ".txt"             => "text/plain",

            ".pdf"             => "application/pdf",

            ".png"             => "image/png",
            ".jpg" or ".jpeg"  => "image/jpeg",
            ".gif"             => "image/gif",
            ".svg"             => "image/svg+xml",
            ".webp"            => "image/webp",
            ".ico"             => "image/x-icon",

            ".mp3"             => "audio/mpeg",
            ".wav"             => "audio/wav",
            ".mp4"             => "video/mp4",
            ".webm"            => "video/webm",

            ".zip"             => "application/zip",
            ".csv"             => "text/csv",

            _                  => "application/octet-stream"
        };
    }
}