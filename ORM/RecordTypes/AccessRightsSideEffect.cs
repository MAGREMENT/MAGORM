using Base.Fields;
using ORM.Abstract;

namespace ORM.RecordTypes;

public class AccessRightsSideEffect : IFieldSideEffect
{
    public object? OnGet<T>(FieldSideEffectEnumerator<T> enumerator, string name) where T : IFieldCollection
    {
        if (false /*TODO*/) throw new Exception();
        return enumerator.NextGet(name);
    }

    public void OnSet<T>(FieldSideEffectEnumerator<T> enumerator, string name, object? value) where T : IFieldCollection
    {
        if (false /*TODO*/) throw new Exception();
        enumerator.NextSet(name, value);
    }
}

public interface IAccessContext
{
    
}