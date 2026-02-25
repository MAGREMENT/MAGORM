using Core.Util;

namespace Core.Abstract;

public abstract class Model : INamed, IDatabaseAttachable
{
    private IReadOnlyList<IFieldDefinition>? _selectFieldDefinitions;
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
    
    public IReadOnlyList<IFieldDefinition> GetFieldDefinitionsForSelect()
    {
        _selectFieldDefinitions ??= DependencyResolutionAlgorithms.Best(AllFieldDefinitions);
        return _selectFieldDefinitions;
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

    public T[] Create<T>(params Dictionary<string, object>[] values) where T : IRecord, new()
    {
        if (_database is null) throw new Exception();

        return _database.CreateRecords<T>(this, values);
    }
    
    public T[] Select<T>(IQueryResult query) where T : IRecord, new() //TODO not IQueryResult
    {
        var result = new T[query.Length];
        var definitions = GetFieldDefinitionsForSelect();

        for (int i = 0; i < query.Length; i++)
        {
            query.Next();

            foreach (var definition in definitions)
            {
                var stringValue = query.TryGetValue(definition.Name, out var v) ? v : null;
                if (!definition.TryComputeValue(stringValue, result[i], out var value)) throw new Exception(); //TODO
                
                result[i].SetFieldValue(definition.Name, value);
            }
        }
        
        return result;
    }
}