using Core.Util;

namespace Core.Abstract;

public abstract class Model : INamed, IDatabaseAttachable
{
    private IReadOnlyList<IFieldDefinition>? _allFieldDefinitions;
    private IReadOnlyList<IFieldDefinition>? _selectFieldDefinitions;
    
    public abstract string Name { get; }
    
    public virtual void AttachToDatabase(Database database)
    {
        foreach (var field in GetAllFieldDefinitions())
        {
            field.AttachToDatabase(database);
        }
    }
    
    public IReadOnlyList<IFieldDefinition> GetFieldDefinitionsForSelect()
    {
        _selectFieldDefinitions ??= DependencyResolutionAlgorithms.Best(GetAllFieldDefinitions());
        return _selectFieldDefinitions;
    }
    
    public CreateSpecification GenerateCreateSpecification(Database database)
    {
        var fields = GetAllFieldDefinitions();
        var fieldSpecifications = new FieldSpecification[fields.Count];
        var fkSpecifications = new List<ForeignKeySpecification>();
        
        for (int i = 0; i < fieldSpecifications.Length; i++)
        {
            var f = fields[i];
            if(!f.IsStored()) continue;
            
            fieldSpecifications[i] = new FieldSpecification(f.Name, f.GetDBFieldType(), 
                f.Options.Unique, f.Options.Required, f.Options.AutoIncrement);

            foreach (var reference in f.References)
            {
                fkSpecifications.Add(new ForeignKeySpecification(f.Name, reference.Model, reference.Field));
            }
        }

        return new CreateSpecification(Name,
            fieldSpecifications, 
            new PrimaryKeySpecification(GetPrimaryKey().Name),
            fkSpecifications);
    }
    
    public abstract IFieldDefinition GetPrimaryKey();

    public abstract IFieldDefinition? GetFieldDefinition(string name);
    
    protected abstract IReadOnlyList<IFieldDefinition> AdditionalFieldDefinitions { get; }
    
    public T[] GenerateRecordsFromSelect<T>(IQueryResult query) where T : IRecord
    {
        var result = CreateRecordArray<T>(query.Length);
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

    protected abstract T[] CreateRecordArray<T>(int length) where T : IRecord; //TODO move elsewhere

    private IReadOnlyList<IFieldDefinition> GetAllFieldDefinitions()
    {
        _allFieldDefinitions ??= AdditionalFieldDefinitions.With(GetPrimaryKey());
        return _allFieldDefinitions;
    }
}