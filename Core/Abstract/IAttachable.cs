namespace Core.Abstract;

public interface IAttachable<in T>
{
    public void Attach(T obj);
    public void Detach(T obj);
}