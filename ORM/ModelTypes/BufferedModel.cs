using Base.Fields;
using ORM.Abstract;

namespace ORM.ModelTypes;

public abstract class BufferedModel : IModel
{
    private readonly List<string> _allFieldsName = new();
    private readonly List<string> _allAutoIncrementFieldsName = new();
    private readonly List<IFieldDefinition> _allAutoIncrementFields = new();
    protected Database? _database;
    
    public abstract string Name { get; }

    internal Database? Database => _database;
    
    public virtual void Attach(Database database)
    {
        _database = database;
        foreach(var field in AllFieldDefinitions) field.Attach(database);
    }

    public void Detach(Database obj)
    {
        _database = null;
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
    
    public T Create<T>(IReadOnlyKeyValue<string, object?> values) where T : IRecord, new()
    {
        return Create<T>([values])[0];
    }

    public T[] Create<T>(params IReadOnlyKeyValue<string, object?>[] values) where T : IRecord, new()
    {
        if (_database is null) throw new Exception();

        return _database.CreateRecords<T>(this, values);
    }
    
    public List<T> Select<T>(QueryCondition? condition = null, params string[] fields) where T : IRecord, new()
    {
        if (_database is null) throw new Exception();

        return fields.Length == 0 
            ? _database.SelectRecords<T>(this, condition)
            : _database.SelectRecords<T>(this, fields, condition);
    }

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