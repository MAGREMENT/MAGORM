using System.Reflection;

namespace ORM.Util;

public static class ReflectionExtensions
{
    public static bool IsTypeNullable(this PropertyInfo prop)
    {
        if (Nullable.GetUnderlyingType(prop.PropertyType) is not null) return true;
        
        var context = new NullabilityInfoContext(); //TODO maybe not create every time ?
        var info = context.Create(prop);
        return info.ReadState == NullabilityState.Nullable;
    }
}