using System.Reflection;

namespace Base.PropertyCollections.Implementations;

public abstract class ClassPropertyCollection : PropertyCollection
{
    protected override void InternalSetValue(string name, object? value)
    {
        var prop = FindProperty(name);
        prop?.SetValue(this, value);
    }

    protected override object? InternalGetValue(string name)
    {
        var prop = FindProperty(name);
        if (prop is null) throw new Exception(ToString() + " does not have a property named " + name);
        
        return prop.GetValue(this);
    }

    private PropertyInfo? FindProperty(string name)
    {
        foreach (var prop in GetType().GetProperties())
        {
            if (prop.Name == name)
            {
                if (!Attribute.IsDefined(prop, typeof(TrackedPropertyAttribute)))
                {
                    throw new Exception("Cannot get or set an untracked property");
                }

                return prop;
            }
        }

        return null;
    }
}