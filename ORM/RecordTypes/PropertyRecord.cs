using Base.Fields.Implementations;
using ORM.Abstract;

namespace ORM.RecordTypes;

public abstract class PropertyRecord : PropertyFieldCollection, IRecord
{
    public void Init(string name, object? value) => Set(name, value);

    public abstract IModel GetModel();
}

public class ModelDefinitionAttribute : PropertyFieldCollectionAttribute;

public class ModelFieldAttribute : FieldAttribute;