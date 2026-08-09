using API;
using Base;
using ORM.Abstract;

namespace Composition;

public record Module(
    string Name,
    IReadOnlyList<string> Dependencies,
    IReadOnlyList<IModel> Models,
    IReadOnlyList<IEndpointProvider> Endpoints,
    IReadOnlyList<string> StaticFiles) : INamed
{
    public virtual bool Equals(Module? other)
    {
        return other?.Name == Name;
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}