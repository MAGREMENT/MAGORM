using Core.Util;

namespace Core.Abstract;

public abstract class AbstractModel : INamed
{
    private IReadOnlyList<IFieldDefinition>? _allFieldDefinitions;
    private IReadOnlyList<IFieldDefinition>? _selectFieldDefinitions;
    
    public abstract string Name { get; }
    
    public IReadOnlyList<IFieldDefinition> GetFieldDefinitionsForSelect()
    {
        _selectFieldDefinitions ??= DependencyResolutionAlgorithms.Best(GetAllFieldDefinitions());
        return _selectFieldDefinitions;
    }
    
    public CreateSpecification GenerateCreateSpecification()
    {
        var fields = GetAllFieldDefinitions();
        var fieldSpecifications = new FieldSpecification[fields.Count];
        var fkSpecifications = new List<ForeignKeySpecification>();
        
        for (int i = 0; i < fieldSpecifications.Length; i++)
        {
            var f = fields[i];
            fieldSpecifications[i] = new FieldSpecification(f.Name, f.GetFieldType(), 
                f.Options.Unique, f.Options.Required, f.Options.AutoIncrement);
        }

        return new CreateSpecification(Name,
            fieldSpecifications, 
            new PrimaryKeySpecification(GetPrimaryKey().Name),
            fkSpecifications);
    }
    
    public abstract IFieldDefinition GetPrimaryKey();
    
    protected abstract IReadOnlyList<IFieldDefinition> AdditionalFieldDefinitions { get; }

    private IReadOnlyList<IFieldDefinition> GetAllFieldDefinitions()
    {
        _allFieldDefinitions ??= AdditionalFieldDefinitions.With(GetPrimaryKey());
        return _allFieldDefinitions;
    }
}

public abstract class AbstractModel<T> : AbstractModel where T : IRecord
{
    internal T[] GenerateRecordsFromSelect(IQueryResult query)
    {
        var result = CreateRecordArray(query.Length);
        var definitions = GetFieldDefinitionsForSelect();

        for (int i = 0; i < query.Length; i++)
        {
            query.Next();

            foreach (var definition in definitions)
            {
                var stringValue = query.TryGetValue(definition.Name, out var v) ? v : null;
                if (!definition.TryFetchValue(stringValue, result[i], out var value)) throw new Exception(); //TODO
                
                result[i].SetFieldValue(definition.Name, value);
            }
        }
        
        return result;
    }

    protected abstract T[] CreateRecordArray(int length);
}

public abstract class TrackedAbstractModel<T> : AbstractModel<T>, ITrackingContext where T : TrackableRecord, new()
{
    internal Database? Database { get; set; }
    
    protected override T[] CreateRecordArray(int length)
    {
        var result = new T[length];
        for (int i = 0; i < length; i++)
        {
            result[i].IsTracked = true;
            result[i].Context = this;
        }

        return result;
    }

    public void AddToDirty(IRecord record)
    {
        if (Database is null) throw new Exception(); //TODO
        
        Database.AddToDirty(this, record);
    }
}