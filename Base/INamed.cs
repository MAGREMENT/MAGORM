namespace Base;

public interface INamed
{
    public string Name { get; }
}

public static class NamedExtensions
{
    public static IEnumerable<string> EnumerateNames(this IEnumerable<INamed> namedEnumerable)
        => namedEnumerable.Select(e => e.Name);
}