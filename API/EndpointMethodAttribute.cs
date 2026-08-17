using System.Linq.Expressions;
using System.Reflection;

namespace API;

[AttributeUsage(AttributeTargets.Method)]
public class EndpointMethodAttribute(EndpointType type, string route) : Attribute
{
    public EndpointType Type { get; } = type;

    public string Route { get; } = route;
}

public static class EndpointMethod
{
    public static List<Endpoint> GetEndpoints(Type type)
    {
        List<Endpoint> endpoints = new();
        foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public))
        {
            var attr = method.GetCustomAttribute<EndpointMethodAttribute>();
            if(attr is null) continue;
            
            var parameters = method.GetParameters();
            var types = new Type[parameters.Length + 1];
            for (int i = 0; i < parameters.Length; i++)
            {
                types[i] = parameters[i].ParameterType;
            }
            types[^1] = method.ReturnType;
            var del = method.CreateDelegate(Expression.GetDelegateType(types));
            
            endpoints.Add(new Endpoint(attr.Type, attr.Route, del, [])); //TODO checks
        }

        return endpoints;
    }
}