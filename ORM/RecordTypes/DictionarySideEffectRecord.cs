using Base.Fields.Implementations;
using ORM.Abstract;

namespace ORM.RecordTypes;

public class DictionarySideEffectRecord : DictionarySideEffectFieldCollection, IRecord
{
    public void Init(string name, object? value)
    {
        InternalSetValue(name, value);
    }

    public bool TryGet(string name, out object? value)
    {
        return _dic.TryGetValue(name, out value);
    }
}