using Base;
using Base.Fields;

namespace ORM.Abstract;

public interface IRecord : IFieldCollection, IKeyValue<string, object?>
{
    void Init(string name, object? value);
}

public static class RecordExtensions
{
    public static bool IsFullyEqualTo(this IRecord record1, IRecord record2)
    {
        if (record1.GetFieldCount() != record2.GetFieldCount()) return false;

        foreach (var name in record1.GetFieldsName())
        {
            if (!record2.TryGet(name, out var value) ||
                !record1.Get(name).NullableEquals(value)) return false;
        }
        
        return true;
    }
    
    public static bool AreCommonFieldsEqual(this IRecord record1, IRecord record2)
    {
        foreach (var name in record1.GetFieldsName())
        {
            if (!record2.TryGet(name, out var value)) continue;

            if(!record1.Get(name).NullableEquals(value)) return false;
        }

        return true;
    }
}