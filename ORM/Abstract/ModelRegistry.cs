using Base.Dependency;
using ORM.ModelTypes;
using ORM.Queries;
using ORM.Queries.Builder;
using ORM.Queries.Specifications;

namespace ORM.Abstract;

public interface IReadOnlyModelRegistry
{
    IModel? GetModel(string name);
    IEnumerable<IModel> EnumerateModels();
}

public abstract class ModelRegistry : IReadOnlyModelRegistry
{
    public void AddModels(IEnumerable<IModel> models)
    {
        AddModelsInternal(models);
        foreach (var model in models)
        {
            model.Attach(this);
        }
    }

    protected abstract void AddModelsInternal(IEnumerable<IModel> models);
    
    public abstract IModel? GetModel(string name);

    public abstract IEnumerable<IModel> EnumerateModels();
}

public readonly struct ModelScriptBuildingContext(IScriptBuilder builder, ISqlLanguage language)
{
    public readonly IScriptBuilder Builder = builder;
    public readonly ISqlLanguage Language = language;
}

public static class ModelHandling
{
    public static void InsertRecords<TRecord>(ModelScriptBuildingContext context, IModel model,
        IEnumerable<TRecord> records)
        where TRecord : IRecord
    {
        var parameters = new List<object?>();
        var fieldNames = new List<string>();

        foreach (var record in records)
        {
            parameters.Clear();
            fieldNames.Clear();
            foreach (var field in model.AllFieldDefinitions)
            {
                if (field.Options.AutoIncrement) continue;
                
                if ((!record.TryGet(field.Name, out var v) || v is null) &&
                    field.Options.Required) throw new MissingFieldException(field);
                
                fieldNames.Add(field.Name);
                parameters.Add(field.ToDbValue(v));
            }
            
            context.Language.Insert(context.Builder, new InsertSpecification(model.Name, fieldNames.ToArray(),
                parameters.ToArray(), model.GetAllAutoIncrementFieldsName()));
        }
    }

    //TODO to single
    public static void UpdateRecords(ModelScriptBuildingContext context, IEnumerable<RecordUpdate> changes)
    {
        foreach (var change in changes)
        {
            var parameters = new List<object?>();
            foreach (var fieldName in change.Fields)
            {
                var field = change.Model.GetFieldDefinition(fieldName);
                if (field is null) throw new Exception(); //TODO

                parameters.Add(change.Record.Get(field.Name));
            }
            
            context.Language.Update(context.Builder, new UpdateSpecification(change.Model.Name, change.Fields.ToArray(), parameters));
        }
    }

    //TODO to single
    public static void DeleteRecords(ModelScriptBuildingContext context, IEnumerable<RecordDelete> deletes)
    {
        foreach (var delete in deletes)
        {
            List<QueryCondition> conditions = new();
            var primary = delete.Model.GetPrimaryKey();
            foreach (var record in delete.Records)
            {
                conditions.Add(new QueryCondition(primary.Name, DBOperator.EQUAL, record.Get(primary.Name)));
            }

            var whereSpecification = Conditions.Or(conditions).Compile();
            context.Language.Delete(context.Builder, new DeleteSpecification(delete.Model.Name, whereSpecification));
        }
    }

    public static void SelectRecords(ModelScriptBuildingContext context, SelectTree tree, QueryCondition? where)
    {
        var whereSpecification = where?.Compile();
        List<string> tempTables = [];
        
        foreach (var model in tree.ModelsInDependencyOrder)
        {
            var info = tree[model];
            if (info.DependsOn.Count == 0)
            {
                var tempName = model.Name + "_results";
                context.Language.CreateFromSelect(context.Builder, new CreateFromSelectSpecification(tempName, 
                    new SelectSpecification(model.Name, info.Fields, whereSpecification,
                        []), true));
                context.Language.Select(context.Builder, new SelectSpecification(tempName, ISelectFieldSpecification.Star));
                tempTables.Add(tempName);
            }
            else
            {
                var modelConditions = new QueryCondition[info.DependsOn.Count];
                var pkName = model.GetPrimaryKey().Name; //TODO need to handle situation where pk is not the referenced field
                var i = 0;
                var subQueryBuilder = context.Builder.New();
                foreach (var modelRef in info.DependsOn)
                {
                    subQueryBuilder.Reset();
                    context.Language.Select(subQueryBuilder, new SelectSpecification(modelRef.Model + "_results", [modelRef.Field]));
                    modelConditions[i++] = new QueryCondition(pkName, DBOperator.IN, subQueryBuilder.ToQuery());
                }
                
                context.Language.Select(context.Builder, new SelectSpecification(model.Name, info.Fields, 
                    Conditions.Or(modelConditions).Compile(), []));

            }
        }

        foreach (var tempTable in tempTables)
        {
            context.Language.Drop(context.Builder, tempTable);
        }
    }
}

public record RecordUpdate(IModel Model, IRecord Record, IEnumerable<string> Fields);

public record RecordDelete(IModel Model, IEnumerable<IRecord> Records);

//TODO test if UniqueList is better
public class SelectTreeModelInfo
{
    public readonly HashSet<ModelReference> DependsOn = [];
    public readonly HashSet<IFieldDefinition> Fields = [];
}

public class SelectTree : Dictionary<IModel, SelectTreeModelInfo>, IDependencyCollection<IModel>
{
    private IModel[]? _modelsInDependencyOrder;

    public IModel[] ModelsInDependencyOrder
    {
        get
        {
            _modelsInDependencyOrder ??= DependencyResolutionAlgorithms.Best(this);
            return _modelsInDependencyOrder;
        }
    }

    public IModel? AddModelField(IModel model, IFieldDefinition field)
    {
        var info = GetInfo(model);
        info.Fields.Add(field);
        if (field.Reference is not null)
        {
            var referenceInfo = GetInfo(field.Reference.Model);
            referenceInfo.DependsOn.Add(new ModelReference(model, field));
            _modelsInDependencyOrder = null;
            referenceInfo.Fields.Add(field.Reference.Field);
            return field.Reference.Model;
        }

        return null;
    }

    public void AddModelFields(IModel model, IReadOnlyList<IFieldDefinition> fields)
    {
        var info = GetInfo(model);
        info.Fields.UnionWith(fields);
        foreach (var field in fields)
        {
            if (field.Reference is not null)
            {
                var referenceInfo = GetInfo(field.Reference.Model);
                referenceInfo.DependsOn.Add(new ModelReference(model, field));
                _modelsInDependencyOrder = null;
                referenceInfo.Fields.Add(field.Reference.Field);
            }
        }
    }

    public void AddFromStrings(IModel startModel, IReadOnlyList<string> strings)
    {
        foreach (var f in strings)
        {
            var currIndex = 0;
            int nextIndex;
            IModel? currModel = startModel;
            IFieldDefinition? currField;
            while ((nextIndex = f.IndexOf('.', currIndex)) >= 0)
            {
                if (currModel is null) throw new Exception(); //TODO
                currField = startModel.GetFieldDefinition(f.AsSpan(currIndex, nextIndex));
                if (currField is null) throw new Exception(); //TODO
                currModel = AddModelField(currModel, currField);
                
                currIndex = nextIndex + 1;
            }

            if (currModel is null) throw new Exception(); //TODO
            currField = currModel.GetFieldDefinition(f.AsSpan(currIndex));
            if (currField is null) throw new Exception(); //TODO
            AddModelField(currModel, currField);
        }
    }

    public IEnumerable<IModel> Enumerate() => Keys;

    public IEnumerable<string> GetDependsOn(IModel named) => this[named].DependsOn.Select(mr => mr.Model.Name);

    public int GetDependsOnCount(IModel named) => this[named].DependsOn.Count;

    private SelectTreeModelInfo GetInfo(IModel model)
    {
        if (!TryGetValue(model, out var info))
        {
            info = new SelectTreeModelInfo();
            this[model] = info;
        }

        return info;
    }
}