using Base.Fields.Implementations;
using ORM.Abstract;
using ORM.ModelTypes;

namespace ORM.RecordTypes;

public class DictionaryRecord : DictionaryFieldCollection, IRecord
{
    public void Init(string name, object? value)
    {
        Set(name, value);
    }

    public IModel? GetModel() => null;
}