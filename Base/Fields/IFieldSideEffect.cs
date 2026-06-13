namespace Base.Fields;

public interface IFieldSideEffect
{
    object? OnGet<T>(FieldSideEffectEnumerator<T> enumerator, string name) where T : IFieldCollection;

    void OnSet<T>(FieldSideEffectEnumerator<T> enumerator, string name, object? value) where T : IFieldCollection;
}