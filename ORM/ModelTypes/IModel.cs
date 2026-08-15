using Base;
using Base.Fields;
using ORM.Queries.Specifications;

namespace ORM.Abstract;

public interface IModel : INamed, IAttachable<Database>
{
    IReadOnlyList<string> GetAllAutoIncrementFieldsName();
    
    IReadOnlyList<IFieldDefinition> GetAllAutoIncrementFields();
    
    public IFieldDefinition GetPrimaryKey();
    
    public IFieldDefinition? GetFieldDefinition(string name);
    
    public IFieldDefinition? GetFieldDefinition(ReadOnlySpan<char> name);
    
    public IReadOnlyCollection<IFieldDefinition> AllFieldDefinitions { get; }

    public T Create<T>(IReadOnlyKeyValue<string, object?> values) where T : IRecord, new();

    public T[] Create<T>(params IReadOnlyKeyValue<string, object?>[] values) where T : IRecord, new();

    public List<T> Select<T>(QueryCondition? condition = null, params string[] fields) where T : IRecord, new();

    public IRecord InstantiateRecord();
}

public static class ModelExtensions
{
    public static CreateSpecification GenerateSpecification(this IModel model)
    {
        var fieldSpecifications = new FieldSpecification[model.AllFieldDefinitions.Count];
        var fkSpecifications = new List<ForeignKeySpecification>();

        var i = 0;
        foreach(var f in model.AllFieldDefinitions)
        {
            fieldSpecifications[i++] = new FieldSpecification(f.Name, f.GetDBFieldType(), 
                f.Options.Unique, f.Options.Required, f.Options.AutoIncrement);

            if (f.References is not null)
            {
                fkSpecifications.Add(new ForeignKeySpecification(f.Name, f.References.Model, f.References.Field));
            }
        }

        return new CreateSpecification(model.Name,
            fieldSpecifications,
            new PrimaryKeySpecification(model.GetPrimaryKey().Name),
            fkSpecifications);
    }
}