namespace Base.Services;

public interface IServiceProvider
{
    public object GetService(Type t);
}

public static class ServiceProviderExtensions
{
    public static T GetService<T>(this IServiceProvider provider)
    {
        return (T)provider.GetService(typeof(T));
    }
}