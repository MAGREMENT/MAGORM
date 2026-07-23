using Base.Fields.Implementations;
using ORM.Abstract;

namespace ORM.RecordTypes;

public class DictionaryRecord : DictionaryFieldCollection, IRecord
{
    public void Init(string name, object? value)
    {
        Set(name, value);
    }
}