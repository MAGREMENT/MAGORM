namespace Base.Extensibility;

public interface IExtensionCollection<T>
{
    int BaseState { get; }

    void Add(T extension);
}

public class ListExtensionCollection<T> : List<T>, IExtensionCollection<T>
{
    public int BaseState => 0;
}