namespace Base;

public interface IDependencies : INamed
{
    public IReadOnlyList<string> DependsOn { get; }
}