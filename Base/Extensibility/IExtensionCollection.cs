namespace Base.Extensibility;

public interface IExtensionCollection<T, TMethodIndentifier> where T : IExtension<TMethodIndentifier>
{
    int BaseState { get; }
    
    void Add(T extension);

    T? Next(int state, TMethodIndentifier methodIndentifier);
}

public class ListExtensionCollection<T> : List<T>, IExtensionCollection<T, int> where T : class, IExtension<int>
{
    protected readonly List<List<int>?> _affectedMethods = new();
    
    public int BaseState => 0;

    public new void Add(T extension)
    {
        foreach (var n in extension.GetAffectedMethods())
        {
            while (n >= _affectedMethods.Count)
            {
                _affectedMethods.Add([]);
            }
            _affectedMethods[n]!.Add(_affectedMethods[n]!.Count);
        }
        base.Add(extension);
    }

    public T? Next(int state, int methodIndentifier)
    {
        if (methodIndentifier >= _affectedMethods.Count) return null;
        
        var list = _affectedMethods[methodIndentifier];
        if (list is null) return null;

        if (state >= list.Count) return null;
        return this[list[state]];
    }
}