using System.Text.Json;
using Bridge.ORM.APIConnector;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ORM.Abstract;

namespace API.ASP.NET.CORE;

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
    public void Define(Endpoint endpoint, EndpointOperation operation)
    {
        var builder = _app.MapMethods(endpoint.Route, [endpoint.Type.ToString()], async (HttpContext context) =>
        {
            var body = await context.Request.ReadFromJsonAsync<JsonElement>();
            RecordDictionary values = new();
            foreach (var parameter in endpoint.Parameters)
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
}