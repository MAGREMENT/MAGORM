using Base;
using Base.Dependency;
using Base.Fields;
using ORM.Queries;
using ORM.Queries.Specifications;

namespace ORM.Abstract;

public class Database
{
    private readonly IDatabaseEngine _engine;
    private readonly IModelBank _modelBank;

    public Database(IDatabaseEngine engine, IModelBank modelBank)
    {
        _engine = engine;
        _modelBank = modelBank;
    }

    public void AddModels(params IModel[] models) => AddModels((IEnumerable<IModel>) models);

    public void AddModels(IEnumerable<IModel> models)
    {
        _modelBank.AddModels(models);
        foreach (var model in models)
        {
            model.Attach(this);
        }
    }

    public IEnumerable<IModel> EnumerateModels() => _modelBank.EnumerateModels();
    
    public IModel? GetModel(string name) => _modelBank.GetModel(name);
    
    public void Sync()
    {
        var currentSpecifications = new Dictionary<string, CreateSpecification>();
        
        foreach (var s in _engine.GetModelSchemas())
        {
            currentSpecifications.Add(s.Model, s);
        }

        var stacker = _engine.Language.InitQueryBuilder();
        foreach (var desiredModel in _modelBank.EnumerateModels())
        {
            var desiredSpecification = desiredModel.GenerateSpecification();
            if (!currentSpecifications.TryGetValue(desiredSpecification.Model, out var currSpecification))
            {
                stacker.Create(desiredSpecification);
            }
            else
            {
                //TODO update columns
            }
        }

        var transaction = _engine.CreateTransaction();
        transaction.Execute(stacker.ToQueries());
        transaction.Commit();
    }

    public void Nuke()
    {
        _engine.DropAllTables();
    }

    public TRecord[] CreateRecords<TRecord>(IModel model, params IReadOnlyKeyValue<string, object?>[] values)
        where TRecord : IRecord, new()
    {
        var result = new TRecord[values.Length];
        var stacker = _engine.Language.InitQueryBuilder();
        var fieldNames = new List<string>();
        var fields = model.AllFieldDefinitions;
        var parameters = new List<object>();
        
        for(int i = 0; i < values.Length; i++)
        {
            parameters.Clear();
            var val = values[i];

            var record = new TRecord();
            result[i] = record;
            
            foreach (var field in fields)
            {
                if (!val.TryGet(field.Name, out var v))
                {
                    if(field.Options.AutoIncrement) continue;
                    if (field.Options.Required) throw new MissingFieldException(field);
                }
                
                if (!field.TryComputeValue(v, record, out var r)) throw new Exception(); //TODO
                    
                fieldNames.Add(field.Name);
                record.Init(field.Name, r);
                parameters.Add(r!);
            }
            
            stacker.Insert(new InsertSpecification(model.Name, fieldNames.ToArray(),
                    model.GetAllAutoIncrementFieldsName()), parameters);
        }

        var transaction = _engine.CreateTransaction();
        transaction.ExecuteResult(queryResult =>
        {
            var index = 0;
            do
            {
                queryResult.Next();
                foreach (var field in model.GetAllAutoIncrementFields())
                {
                    var objValue = queryResult.TryGet(field.Name, out var v) ? v : null;
                    if (!field.TryComputeValue(objValue, result[index], out var value)) throw new Exception(); //TODO

                    result[index].Init(field.Name, value);
                }
            } while (queryResult.NextResultSet());

            return true;
        }, stacker.ToQueries());
        transaction.Commit();
        
        return result;
    }

    public List<T> SelectRecords<T>(IModel model, QueryCondition? where)
        where T : IRecord, new()
    {
        var mf = new SelectTree();
        mf.AddModelFields(model, ModelReference.None, model.AllFieldDefinitions.ToArray());
        return SelectRecords<T>(mf, where);
    }

    public List<T> SelectRecords<T>(IModel model, IReadOnlyList<string> fields, QueryCondition? where)
        where T : IRecord, new()
    {
        var mf = new SelectTree();
        mf.AddFromStrings(model, _modelBank, fields);
        return SelectRecords<T>(mf, where);
    }

    public void UpdateRecords(IEnumerable<RecordUpdate> changes)
    {
        var stacker = _engine.Language.InitQueryBuilder();
        
        foreach (var change in changes)
        {
            var parameters = new List<object?>();
            foreach (var fieldName in change.Fields)
            {
                var field = change.Model.GetFieldDefinition(fieldName);
                if (field is null) throw new Exception(); //TODO

                parameters.Add(change.Record.Get(field.Name));
            }
            
            stacker.Update(new UpdateSpecification(change.Model.Name, change.Fields.ToArray()), parameters);
        }

        _engine.Execute(stacker.ToQueries());
    }

    public void DeleteRecords(IEnumerable<RecordDelete> deletes)
    {
        var stacker = _engine.Language.InitQueryBuilder();
        foreach (var delete in deletes)
        {
            List<QueryCondition> conditions = new();
            var primary = delete.Model.GetPrimaryKey();
            foreach (var record in delete.Records)
            {
                conditions.Add(new QueryCondition(primary.Name, DBOperator.EQUAL, record.Get(primary.Name)));
            }

            var (whereSpecification, parameters) = Conditions.Or(conditions).Compile();
            stacker.Delete(new DeleteSpecification(delete.Model.Name, whereSpecification), parameters);
        }
        
        _engine.Execute(stacker.ToQueries());
    }

    private List<T> SelectRecords<T>(SelectTree tree, QueryCondition? where)
        where T : IRecord, new()
    {
        WhereSpecification? whereSpecification;
        IReadOnlyList<object?> parameters;
        List<string> tempTables = [];
        if (where is null)
        {
            whereSpecification = null;
            parameters = [];
        } else (whereSpecification, parameters) = where.Compile();

        var stacker = _engine.Language.InitQueryBuilder();
        var modelsInDependencyOrder = DependencyResolutionAlgorithms.Best(tree);
        foreach (var model in modelsInDependencyOrder)
        {
            var info = tree[model];
            if (info.References.Count == 0)
            {
                var tempName = model.Name + "_results";
                stacker.CreateFromSelect(new CreateFromSelectSpecification(tempName, 
                    new SelectSpecification(model.Name, info.Fields.EnumerateNames(), whereSpecification,
                    []), true), parameters);
                stacker.Select(new SelectSpecification(tempName, info.Fields.EnumerateNames()));
                tempTables.Add(tempName);
            }
            else
            {
                var modelConditions = new QueryCondition[info.References.Count];
                var pkName = model.GetPrimaryKey().Name; //TODO need to handle situation where pk is not the referenced field
                var i = 0;
                var subQueryBuilder = _engine.Language.InitQueryBuilder();
                foreach (var modelRef in info.References)
                {
                    subQueryBuilder.Reset();
                    subQueryBuilder.Select(new SelectSpecification(modelRef.Model + "_results", [modelRef.Field]));
                    modelConditions[i++] = new QueryCondition(pkName, DBOperator.IN, subQueryBuilder.ToQuery());
                }
                stacker.Select(new SelectSpecification(model.Name, info.Fields.EnumerateNames(), 
                    Conditions.Or(modelConditions).Compile().Item1, []));

            }
        }

        foreach (var tempTable in tempTables)
        {
            stacker.Drop(tempTable);
        }

        return _engine.ExecuteResult<List<T>>(queryResult => CreateRecordsFromQueryResult<T>(queryResult,
                modelsInDependencyOrder, tree), stacker.ToQueries());
    }
    
    private static List<T> CreateRecordsFromQueryResult<T>(IQueryResult queryResult,
        IReadOnlyList<IModel> modelsInOrder, SelectTree tree)
        where T : IRecord, new()
    {
        var i = 0;
        var result = new List<T>();
        // (Model name, Record Primary Key) (Needing Reference Record, Needing Reference Field)
        var needs = new Dictionary<(string, object?), List<(IRecord, string)>>();
        do
        {
            var model = modelsInOrder[i];
            var info = tree[model];
            
            while (queryResult.Next())
            {
                IRecord record;
                if (i == 0)
                {
                    var resultRecord = new T();
                    result.Add(resultRecord);
                    record = resultRecord;
                }
                else record = model.InstantiateRecord();

                List<(IRecord, string)>? needingReferenceList;
                foreach (var definition in info.Fields)
                {
                    var objValue = queryResult.TryGet(definition.Name, out var v) ? v : null;
                    if (!definition.TryComputeValue(objValue, record, out var value)) throw new Exception(); //TODO
                
                    record.Init(definition.Name, value);
                    if (definition.References is not null)
                    {
                        if (!needs.TryGetValue((definition.References.Model, value), out needingReferenceList))
                        {
                            needingReferenceList = new List<(IRecord, string)>();
                            needs[(definition.References.Model, value)] = needingReferenceList;
                        }
                        needingReferenceList.Add((record, definition.Name));
                    }
                }
                
                var pk = model.GetPrimaryKey(); //TODO need to handle situation where pk is not the referenced field
                if (record.TryGet(pk.Name, out var pkValue) && needs.TryGetValue((model.Name, pkValue), out needingReferenceList))
                {
                    foreach (var (currRecord, fieldName) in needingReferenceList)
                    {
                        currRecord.Set(fieldName, record);
                    }
                    
                    needingReferenceList.Clear();
                }
            }

            i++;
        } while (queryResult.NextResultSet());

        return result;
    }
}

public record RecordUpdate(IModel Model, IRecord Record, IEnumerable<string> Fields);

public record RecordDelete(IModel Model, IEnumerable<IRecord> Records);

//TODO test if UniqueList is better
public class SelectTreeModelInfo
{
    public readonly HashSet<ModelReference> References = [];
    public readonly HashSet<IFieldDefinition> Fields = [];
}

public class SelectTree : Dictionary<IModel, SelectTreeModelInfo>, IDependencyCollection<IModel>
{
    public void AddModelField(IModel model, ModelReference modelRef, IFieldDefinition field)
    {
        var info = GetInfo(model);
        if(!modelRef.IsNone()) info.References.Add(modelRef);
        info.Fields.Add(field);
    }

    public void AddModelFields(IModel model, ModelReference modelRef, IReadOnlyList<IFieldDefinition> fields)
    {
        var info = GetInfo(model);
        if(!modelRef.IsNone()) info.References.Add(modelRef);
        info.Fields.UnionWith(fields);
    }

    public void AddFromStrings(IModel startModel, IModelBank bank, IReadOnlyList<string> strings)
    {
        foreach (var f in strings)
        {
            var currIndex = 0;
            int nextIndex;
            var currModel = startModel;
            IFieldDefinition? currField;
            var currRef = ModelReference.None;
            while ((nextIndex = f.IndexOf('.', currIndex)) >= 0)
            {
                currField = startModel.GetFieldDefinition(f.AsSpan(currIndex, nextIndex));
                if (currField is null) throw new Exception(); //TODO
                if (currField.References is null) throw new Exception(); //TODO

                var oldModel = currModel;
                var oldRef = currRef;
                currModel = bank.GetModel(currField.References.Model);
                if (currModel is null) throw new Exception(); //TODO
                
                currRef = new ModelReference(oldModel.Name, currField.Name);
                AddModelField(oldModel, oldRef, currField);
                AddModelField(currModel, currRef, currModel.GetPrimaryKey());
                currIndex = nextIndex + 1;
            }

            currField = currModel.GetFieldDefinition(f.AsSpan(currIndex));
            if (currField is null) throw new Exception(); //TODO

            AddModelField(currModel, currRef, currField);
        }
    }

    public IEnumerable<IModel> Enumerate() => Keys;

    public IEnumerable<string> GetDependsOn(IModel named) => this[named].References.Select(mr => mr.Model);

    public int GetDependsOnCount(IModel named) => this[named].References.Count;

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

public class MissingFieldException(IFieldDefinition definition) 
    : Exception($"The field {definition.Name} is required and missing from the values");