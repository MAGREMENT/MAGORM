using System.Reflection.Metadata;
using System.Text.Json;
using Bridge.ORM.API;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ORM.Abstract;

namespace API.ASP.NET.CORE;

public static class WebApplicationExtensions
{
    public static void MapModels(this WebApplication app, Database database)
    {
        var definer = new WebApplicationEndpointDefiner(app);
        database.AddModelsEndpoints(definer);
    }
}

public class WebApplicationEndpointDefiner(WebApplication _app) : IEndpointDefiner
{
    public void Define(Endpoint endpoint, IEnumerable<EndpointParameter> parameters, EndpointOperation operation)
    {
        var builder = _app.MapMethods(endpoint.Route, [endpoint.Type.ToString()], async (HttpContext context) =>
        {
            var body = await context.Request.ReadFromJsonAsync<JsonElement>();
            RecordDictionary values = new();
            foreach (var parameter in parameters)
            {
                if (!body.TryGetProperty(parameter.Name, out var jsonValue))
                    return Results.BadRequest($"Parameter {parameter.Name} missing");

                object? value;
                try
                {
                    value = JsonSerializer.Deserialize(jsonValue.GetRawText(), parameter.Type);
                }
                catch(JsonException)
                {
                    return Results.BadRequest($"Parameter {parameter.Name} has wrong type, should be {parameter.Type.Name}");
                }

                if (parameter.Check is not null && !parameter.Check(value))
                    return Results.BadRequest($"Parameter {parameter.Name} is invalid");

                values.Add(parameter.Name, value);
            }

            operation(values);
            return Results.Ok();
        }).Accepts(typeof(Dictionary<string, object?>), "application/json");
    }

    public void Define<T>(Endpoint endpoint, T operation) where T : Delegate
    {
        _app.MapGet(endpoint.Route, operation)
            .AddEndpointFilter(HandleEndpointResult);
    }

    public void AddDefaultHandler(DefaultHandler handler)
    {
        _app.Use(async (context, next) =>
        {
            await next(context);

            if (context.Response is { StatusCode: 404, HasStarted: false })
            {
                var result = handler(context.Request.Path);
                if (result is string s)
                {
                    var r = Results.File(s, GetContentType(Path.GetExtension(s)));
                    context.Response.StatusCode = 200;
                    await r.ExecuteAsync(context);
                }
            }
        });
    }

    private static async ValueTask<object?> HandleEndpointResult(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var result = await next(context);
        //TODO correct result handling
        if (result is string s)
        {
            return Results.File(s, GetContentType(Path.GetExtension(s)));
        }
        
        return result;
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