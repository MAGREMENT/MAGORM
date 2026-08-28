using ORM.Abstract;

namespace ORM.ModelTypes;

public abstract class BufferedModel : IModel
{
    private readonly List<string> _allFieldsName = new();
    private readonly List<string> _allAutoIncrementFieldsName = new();
    private readonly List<IFieldDefinition> _allAutoIncrementFields = new();
    
    public abstract string Name { get; }
    
    public virtual void Attach(IReadOnlyModelRegistry database)
    {
        foreach(var field in AllFieldDefinitions) field.Attach(database);
    }

    public void Detach(IReadOnlyModelRegistry obj)
    {
        foreach(var field in AllFieldDefinitions) field.Detach(obj);
    }

    public virtual void Add(IFieldDefinition field)
    {
        _allFieldsName.Add(field.Name);
        if (field.Options.AutoIncrement)
        {
            _allAutoIncrementFields.Add(field);
            _allAutoIncrementFieldsName.Add(field.Name);
        }
    }

    public IReadOnlyList<string> GetAllFieldsName() => _allFieldsName;

    public IReadOnlyList<string> GetAllAutoIncrementFieldsName() => _allAutoIncrementFieldsName;
    
    public IReadOnlyList<IFieldDefinition> GetAllAutoIncrementFields() => _allAutoIncrementFields;
    
    public abstract IFieldDefinition GetPrimaryKey();

    public abstract IFieldDefinition? GetFieldDefinition(string name);

    public abstract IFieldDefinition? GetFieldDefinition(ReadOnlySpan<char> name);
    
    public abstract IReadOnlyCollection<IFieldDefinition> AllFieldDefinitions { get; }

    public abstract IRecord InstantiateRecord();

    public override bool Equals(object? obj)
    {
        return obj is BufferedModel m && m.Name == Name;
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    public override string ToString()
    {
        return Name;
    }
}