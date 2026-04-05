using System.Reflection;

namespace ApiConnector;

public class Connector(IEndpointDefiner _definer)
{
    private static readonly Type PropertyType = typeof(IEndpointProperty);
    
    public void AddDefaultEndpoints<T>(object obj, IDefaultEndpointsOperations<T> operations, DefaultEndpoints endpoints)
    {
        Dictionary<string, Type> schema = new();
        var type = obj.GetType();
        foreach (var property in type.GetProperties())
        {
            var endpointProperty = TryGetEndpointAttribute(property);
            if(endpointProperty is null) continue;
            
            schema.Add(property.Name, property.PropertyType);
        }

        for (int i = 0; i < 4; i++)
        {
            var v = 1 << i;
            if((((int)endpoints >> v) & 1) == 0) continue;

            var ep = (DefaultEndpoints)v;
            var endpointType =ep.ToEndpointType();
            _definer.Define(endpointType, type.Name.ToLower() + "/" + endpointType.ToString().ToLower(), schema, 
                ep.GetEndpointOperation(operations));
        }
    }

    private static IEndpointProperty? TryGetEndpointAttribute(PropertyInfo info)
    {
        foreach (var attr in info.CustomAttributes)
        {
            var type = attr.AttributeType;
            if (PropertyType.IsAssignableFrom(type))
            {
                return (IEndpointProperty)info.GetCustomAttribute(type)!;
            }
        }

        return null;
    }
}

public interface IDefaultEndpointsOperations<T>
{
    object? Create(T parameters);
    object? Read(T parameters);
    object? Update(T parameters);
    object? Delete(T parameters);
}

[Flags]
public enum DefaultEndpoints
{
    CREATE = 0b1, 
    READ = 0b10, 
    UPDATE = 0b100, 
    DELETE = 0b1000,
    
    ALL = CREATE | READ | UPDATE | DELETE
}

public static class DefaultEndpointsExtensions
{
    public static EndpointType ToEndpointType(this DefaultEndpoints endpoint)
    {
        return endpoint switch
        {
            DefaultEndpoints.CREATE => EndpointType.POST,
            DefaultEndpoints.READ => EndpointType.GET,
            DefaultEndpoints.UPDATE => EndpointType.SET,
            DefaultEndpoints.DELETE => EndpointType.DELETE,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public static EndpointOperation<T> GetEndpointOperation<T>(this DefaultEndpoints endpoint,
        IDefaultEndpointsOperations<T> operations)
    {
        return endpoint switch
        {
            DefaultEndpoints.CREATE => operations.Create,
            DefaultEndpoints.READ => operations.Read,
            DefaultEndpoints.UPDATE => operations.Update,
            DefaultEndpoints.DELETE => operations.Delete,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}