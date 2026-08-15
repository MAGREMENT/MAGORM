using System.Reflection;
using Base.Fields.Implementations;
using ORM.Abstract;
using ORM.ModelTypes;

namespace ORM.RecordTypes;

public abstract class PropertyRecord : PropertyFieldCollection, IRecord
{
    public void Init(string name, object? value) => Set(name, value);

    public abstract IModel GetModel();
}

public abstract class ModelDefiner
{
    public abstract IModel GetModel();
}

[AttributeUsage(AttributeTargets.Class)]
public class ModelDefinitionAttribute : Attribute
{
    public GenerateModel Generator { get; }
    
    public ModelDefinitionAttribute(Type declaringType, string methodName)
    {
        var method = declaringType.GetMethod(methodName);

        if (method == null) throw new Exception("Generator method not found");

        Generator = (GenerateModel)Delegate.CreateDelegate(typeof(GenerateModel), method);
    }
}

public delegate IModel GenerateModel(Type type);

public static class ModelGenerator
{
    public static IModel Generate(Type type)
    {
        var (primary, definitions) = ModelField.GetFields(type);
        if (primary is null) throw new Exception("Need primary key");
        
        var result = new DictionaryModel(type.Name, primary, definitions);
        return result;
    }
}

public class ModelFieldAttribute : FieldAttribute
{
    public bool Primary { get; set; }
    public bool Required { get; set; }
    public bool Unique { get; set; }
    public bool AutoIncrement { get; set; }
}

public static class ModelField
{
    //TODO reference fields
    public static (IFieldDefinition?, List<IFieldDefinition>) GetFields(Type type)
    {
        IFieldDefinition? primary = null;
        List<IFieldDefinition> definitions = new();
        foreach (var prop in type.GetProperties())
        {
            var attr = prop.GetCustomAttribute<ModelFieldAttribute>();
            if(attr is null) continue;

            var options = new FieldDefinitionsOptions(attr.Required, attr.Unique, attr.AutoIncrement);
            IFieldDefinition field;
            if (prop.PropertyType == typeof(int)) field = Fields.Int(prop.Name, options);
            else if (prop.PropertyType == typeof(string)) field = Fields.String(prop.Name, options);
            else if (prop.PropertyType == typeof(bool)) field = Fields.Bool(prop.Name, options);
            else throw new Exception("Unrecognized Field type");

            if (attr.Primary)
            {
                if (primary is not null) throw new Exception("Duplicate primary key");
                primary = field;
            }
            else definitions.Add(field);
        }

        return (primary, definitions);
    }
}