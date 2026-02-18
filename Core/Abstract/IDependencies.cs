namespace Core.Abstract;

public interface IDependencies : INamed
{
    public IReadOnlyList<string> DependsOn { get; }
}