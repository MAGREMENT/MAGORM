using Base;
using ORM.Abstract;
using ORM.Queries.Specifications;

namespace ORM.ModelTypes;

public interface IModel : INamed, IAttachable<IReadOnlyModelBank>
{
    IReadOnlyList<string> GetAllAutoIncrementFieldsName();
    
    IReadOnlyList<IFieldDefinition> GetAllAutoIncrementFields();
    
    public IFieldDefinition GetPrimaryKey();
    
    public IFieldDefinition? GetFieldDefinition(string name);
    
    public IFieldDefinition? GetFieldDefinition(ReadOnlySpan<char> name);
    
    public IReadOnlyCollection<IFieldDefinition> AllFieldDefinitions { get; }

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

            if (f.Reference is not null)
            {
                fkSpecifications.Add(new ForeignKeySpecification(f.Name, f.Reference.Model.Name, f.Reference.Field.Name));
            }
        }

        return new CreateSpecification(model.Name,
            fieldSpecifications,
            new PrimaryKeySpecification(model.GetPrimaryKey().Name),
            fkSpecifications);
    }
}