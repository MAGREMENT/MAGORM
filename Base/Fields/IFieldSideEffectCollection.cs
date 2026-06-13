namespace Base.Fields;

public interface IFieldSideEffectCollection
{
    public int BaseState { get; }

    public void NextSet<T>(T subject, int state, string name, object? value) where T : IFieldCollection;

    public object? NextGet<T>(T subject, int state, string name) where T : IFieldCollection;
}

public readonly struct FieldSideEffectEnumerator<T>(T subject, IFieldSideEffectCollection _collection, int _state) 
    where T : IFieldCollection
{
    public readonly T Subject = subject;

    public void NextSet(string name, object? value)
    {
        _collection.NextSet(Subject, _state, name, value);
    }

    public object? NextGet(string name)
    {
        return _collection.NextGet(Subject, _state, name);
    }
}