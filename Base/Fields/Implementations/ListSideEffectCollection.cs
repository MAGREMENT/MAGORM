namespace Base.Fields.Implementations;

public class ListFieldSideEffectCollection : IFieldSideEffectCollection
{
    public int BaseState => 0;
    
    private readonly List<IFieldSideEffect> _sideEffects = [];

    public void Add(IFieldSideEffect sideEffect) => _sideEffects.Add(sideEffect);
    
    public void NextSet<T>(T subject, int state, string name, object? value) where T : IFieldCollection
    {
        if(state >= _sideEffects.Count) subject.Set(name, value);
        else _sideEffects[state].OnSet(new FieldSideEffectEnumerator<T>(subject, this, state + 1), name, value); 
    }

    public object? NextGet<T>(T subject, int state, string name) where T : IFieldCollection
    {
        return state >= _sideEffects.Count
            ? subject.Get(name)
            : _sideEffects[state].OnGet(new FieldSideEffectEnumerator<T>(subject, this, state + 1), name);
    }
}

