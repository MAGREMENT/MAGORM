namespace Base.Dependency;

public interface IDependencyCollection<T> where T : INamed
{
    int Count { get; }
    
    IEnumerable<T> Enumerate();

    IEnumerable<string> GetDependsOn(T named);

    int GetDependsOnCount(T named);
}

public class DependencyDictionary<T> : Dictionary<T, HashSet<string>>, IDependencyCollection<T> where T : INamed
{
    public IEnumerable<T> Enumerate() => Keys;

    public int GetDependsOnCount(T named) => this[named].Count;

    public IEnumerable<string> GetDependsOn(T named) => this[named];
    
    public void AddDependency(T named, INamed other)
    {
        throw new NotImplementedException();
    }
}