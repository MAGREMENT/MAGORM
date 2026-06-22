namespace Base.Extensibility;

public interface IExtensionCollection<T> where T : class
{
    int BaseState { get; }
    
    void Add(T extension);

    T? Next(int state, int functionNumber);
}

public class ListExtensionCollection<T> : List<T>, IExtensionCollection<T> where T : class
{
    private readonly List<List<int>?> _affectedExtension = new();
    
    public int BaseState => 0;
    
    public T? Next(int state, int functionNumber)
    {
        var list = _affectedExtension[functionNumber];
        if (list is null) return null;

        if (state >= list.Count) return null;
        return this[list[state]];
    }
}