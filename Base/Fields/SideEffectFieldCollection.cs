using Base.Fields.Implementations;

namespace Base.Fields;

[SideEffectFieldCollection]
public abstract class SideEffectFieldCollection : IFieldCollection
{
    protected IFieldSideEffectCollection _sideEffects = EmptySideEffectCollection.Instance;
    
    public abstract int GetFieldCount();

    public abstract IEnumerable<string> GetFieldsName();

    public void Set(string name, object? value)
    {
        _sideEffects.NextSet(this, _sideEffects.BaseState, name, value);
    }

    public object? Get(string name)
    {
        return _sideEffects.NextGet(this, _sideEffects.BaseState, name);
    }

    public void SetCollection(IFieldSideEffectCollection collection) => _sideEffects = collection;

    protected abstract void InternalSetValue(string name, object? value);

    protected abstract object? InternalGetValue(string name);
}