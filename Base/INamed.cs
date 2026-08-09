namespace Base;

public interface INamed
{
    public string Name { get; }
}

public static class NamedExtensions
{
    public static IEnumerable<string> EnumerateNames(this IEnumerable<INamed> namedEnumerable)
        => namedEnumerable.Select(e => e.Name);

    public static bool ContainsNamed(this IEnumerable<INamed> namedEnumerable, string name)
    {
        foreach (var named in namedEnumerable)
        {
            if (named.Name == name) return true;
        }

        return false;
    }
}