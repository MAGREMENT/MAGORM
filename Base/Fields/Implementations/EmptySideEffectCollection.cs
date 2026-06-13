namespace Base.Fields.Implementations;

public class EmptySideEffectCollection : IFieldSideEffectCollection
{
    public static readonly EmptySideEffectCollection Instance = new();
    
    public int BaseState => 0;
    
    public void NextSet<T>(T subject, int state, string name, object? value) where T : IFieldCollection
    {
        subject.Set(name, value);
    }

    public object? NextGet<T>(T subject, int state, string name) where T : IFieldCollection
    {
        return subject.Get(name);
    }
}