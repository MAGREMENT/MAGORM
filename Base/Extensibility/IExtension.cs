using System.Reflection;

namespace Base.Extensibility;

public interface IExtension<out TMethodIndentifier>
{
    public IEnumerable<TMethodIndentifier> GetAffectedMethods();
}

public abstract class DynamicReflectiveExtension : IExtension<int>
{
    public IEnumerable<int> GetAffectedMethods()
    {
        foreach(var method in GetType().GetMethods())
        {
            if(method.GetBaseDefinition().DeclaringType == method.DeclaringType) continue;
            var n = GetMethodNumber(method);
            if (n >= 0) yield return n;
        }
    }

    protected abstract int GetMethodNumber(MethodInfo method);
}