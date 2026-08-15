namespace Base.Services;

public interface IServiceDependant
{
    public void DefineServices(IServiceCollection collection);
}