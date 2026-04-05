using Base.PropertyCollections.Implementations;
using Core.Abstract;

namespace Core.RecordTypes;

public class DictionaryRecord : DictionaryPropertyCollection, IRecord
{
    public void InitValue(string name, object? value)
    {
        InternalSetValue(name, value);
    }

    public bool TryGetValue(string name, out object? value)
    {
        return _dic.TryGetValue(name, out value);
    }
}