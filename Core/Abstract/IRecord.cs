using Base;
using Base.PropertyCollections;

namespace Core.Abstract;

public interface IRecord : IPropertyCollection
{
    void InitValue(string name, object? value);
    bool TryGetValue(string name, out object? value);
}

public interface IRecordModule : IPropertyCollectionModule;

public abstract class RecordModule : PropertyCollectionModule;

public static class RecordExtensions
{
    public static bool IsFullyEqualTo(this IRecord record1, IRecord record2)
    {
        if (record1.GetPropertyCount() != record2.GetPropertyCount()) return false;

        foreach (var name in record1.GetPropertiesName())
        {
            if (!record2.TryGetValue(name, out var value) ||
                !record1.GetValue(name).NullableEquals(value)) return false;
        }
        
        return true;
    }
    
    public static bool AreCommonFieldsEqual(this IRecord record1, IRecord record2)
    {
        foreach (var name in record1.GetPropertiesName())
        {
            if (!record2.TryGetValue(name, out var value)) continue;

            if(!record1.GetValue(name).NullableEquals(value)) return false;
        }

        return true;
    }
}