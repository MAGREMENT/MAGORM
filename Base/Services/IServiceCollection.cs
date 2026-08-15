namespace Base.Services;

public interface IServiceCollection
{
    public void AddService(Type typeFor, Type typeResult, ServiceInstantiationTactic tactic);
}

public enum ServiceInstantiationTactic
{
    Singleton, New
}

public static class ServiceCollectionExtensions
{
    public static void AddSingleton<TFor, TResult>(this IServiceCollection collection)
    {
        collection.AddService(typeof(TFor), typeof(TResult), ServiceInstantiationTactic.Singleton);
    }
    
    public static void AddNew<TFor, TResult>(this IServiceCollection collection)
    {
        collection.AddService(typeof(TFor), typeof(TResult), ServiceInstantiationTactic.New);
    }
}