using Base;
using ORM.Queries.Specifications;

namespace ORM.Abstract;

public interface IModel : INamed, IAttachable<Database>
{
    void NoticeDirty(IRecord record, string field);
    
    void Add(IFieldDefinition field);
    
    IReadOnlyList<string> GetAllAutoIncrementFieldsName();
    
    IReadOnlyList<IFieldDefinition> GetAllAutoIncrementFields();
    
    CreateSpecification GenerateSpecification();
    
    public IFieldDefinition GetPrimaryKey();
    
    public IFieldDefinition? GetFieldDefinition(string name);
    
    public IFieldDefinition? GetFieldDefinition(ReadOnlySpan<char> name);
    
    public IReadOnlyCollection<IFieldDefinition> AllFieldDefinitions { get; }

    public T Create<T>(IKeyValue<string, object?> values) where T : IRecord, new();

    public T[] Create<T>(params IKeyValue<string, object?>[] values) where T : IRecord, new();

    public List<T> Select<T>(QueryCondition? condition = null, params string[] fields) where T : IRecord, new();

    public IRecord InstantiateRecord();
}

public abstract class Model : IModel
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

    public void NoticeDirty(IRecord record, string field)
    {
        _database?.NoticeDirty(this, record, field);
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
    
    public CreateSpecification GenerateSpecification()
    {
        var fieldSpecifications = new FieldSpecification[AllFieldDefinitions.Count];
        var fkSpecifications = new List<ForeignKeySpecification>();

        var i = 0;
        foreach(var f in AllFieldDefinitions)
        {
            fieldSpecifications[i++] = new FieldSpecification(f.Name, f.GetDBFieldType(), 
                f.Options.Unique, f.Options.Required, f.Options.AutoIncrement);

            if (f.References is not null)
            {
                fkSpecifications.Add(new ForeignKeySpecification(f.Name, f.References.Model, f.References.Field));
            }
        }

        return new CreateSpecification(Name,
            fieldSpecifications,
            new PrimaryKeySpecification(GetPrimaryKey().Name),
            fkSpecifications);
    }
    
    public abstract IFieldDefinition GetPrimaryKey();

    public abstract IFieldDefinition? GetFieldDefinition(string name);

    public abstract IFieldDefinition? GetFieldDefinition(ReadOnlySpan<char> name);
    
    public abstract IReadOnlyCollection<IFieldDefinition> AllFieldDefinitions { get; }
    
    public T Create<T>(IKeyValue<string, object?> values) where T : IRecord, new()
    {
        return Create<T>([values])[0];
    }

    public T[] Create<T>(params IKeyValue<string, object?>[] values) where T : IRecord, new()
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
        return obj is Model m && m.Name == Name;
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