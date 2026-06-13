using Base.Fields;
using ORM.Abstract;

namespace ORM.RecordTypes;

public class TrackedSideEffect(Model model) : IFieldSideEffect //TODO just put this in Model class
{
    public object? OnGet<T>(FieldSideEffectEnumerator<T> enumerator, string name) where T : IFieldCollection
    {
        return enumerator.NextGet(name);
    }

    public void OnSet<T>(FieldSideEffectEnumerator<T> enumerator, string name, object? value) where T : IFieldCollection
    {
        enumerator.NextSet(name, value);
        model.NoticeDirty((IRecord)enumerator.Subject, name);
    }
}