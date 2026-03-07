using Core.Queries;
using Core.Util;

namespace Core.Abstract;

public abstract class Model : INamed, IDatabaseAttachable
{
    private IReadOnlyList<IFieldDefinition>? _dependencyOrderedFieldDefinitions;
    private IReadOnlyList<string>? _allFieldsName;
    private Database? _database;
    
    public abstract string Name { get; }
    
    public virtual void AttachToDatabase(Database database)
    {
        _database = database;
        foreach (var field in AllFieldDefinitions)
        {
            field.AttachToDatabase(database);
        }
    }
    
    public IReadOnlyList<IFieldDefinition> GetDependencyOrderedFieldDefinitions()
    {
        _dependencyOrderedFieldDefinitions ??= DependencyResolutionAlgorithms.Best(AllFieldDefinitions);
        return _dependencyOrderedFieldDefinitions;
    }

    public IReadOnlyList<string> GetAllFieldsName()
    {
        if (_allFieldsName is null)
        {
            var result = new string[AllFieldDefinitions.Count];
            var i = 0;
            foreach (var field in AllFieldDefinitions)
            {
                result[i++] = field.Name;
            }

            _allFieldsName = result;
        }
        return _allFieldsName;
    }
    
    public ModelSpecification GenerateSpecification()
    {
        var fieldSpecifications = new FieldSpecification[AllFieldDefinitions.Count];
        var fkSpecifications = new List<ForeignKeySpecification>();

        var i = 0;
        foreach(var f in AllFieldDefinitions)
        {
            if(!f.IsStored()) continue;
            
            fieldSpecifications[i++] = new FieldSpecification(f.Name, f.GetDBFieldType(), 
                f.Options.Unique, f.Options.Required, f.Options.AutoIncrement);

            foreach (var reference in f.References)
            {
                fkSpecifications.Add(new ForeignKeySpecification(f.Name, reference.Model, reference.Field));
            }
        }

        return new ModelSpecification(Name,
            fieldSpecifications, 
            new PrimaryKeySpecification(GetPrimaryKey().Name),
            fkSpecifications);
    }
    
    public abstract IFieldDefinition GetPrimaryKey();

    public abstract IFieldDefinition? GetFieldDefinition(string name);
    
    public abstract IReadOnlyCollection<IFieldDefinition> AllFieldDefinitions { get; }
    
    public T Create<T>(Dictionary<string, object> values) where T : IRecord, new()
    {
        return Create<T>([values])[0];
    }

    public T[] Create<T>(params Dictionary<string, object>[] values) where T : IRecord, new()
    {
        if (_database is null) throw new Exception();

        return _database.CreateRecords<T>(this, values);
    }
    
    public List<T> Select<T>(QueryCondition? condition = null, params string[] fields) where T : IRecord, new()
    {
        if (_database is null) throw new Exception();

        return _database.SelectRecords<T>(this, fields.Length == 0 ? GetAllFieldsName() : fields, condition);
    }
}