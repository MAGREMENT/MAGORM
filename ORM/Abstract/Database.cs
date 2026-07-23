using Base;
using Base.Dependency;
using Base.Fields;
using ORM.Queries;
using ORM.Queries.Specifications;

namespace ORM.Abstract;

public class Database
{
    private readonly ISqlLanguage _sqlLanguage;
    private readonly IDatabaseEngine _engine;
    private readonly IModelBank _modelBank;
    private readonly DirtyCollection _dirty = new();

    public Database(ISqlLanguage sqlLanguage, IDatabaseEngine engine, IModelBank modelBank)
    {
        _sqlLanguage = sqlLanguage;
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

    public void NoticeDirty(IModel model, IRecord record, string field)
    {
        _dirty.AddToDirty(model, record, field);
    }

    public void Sync()
    {
        var currentSpecifications = new Dictionary<string, CreateSpecification>();
        
        foreach (var s in _engine.GetModelSchemas())
        {
            currentSpecifications.Add(s.Model, s);
        }

        var stacker = _sqlLanguage.InitQueryBuilder();
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

    public TRecord[] CreateRecords<TRecord>(IModel model, params IKeyValue<string, object?>[] values)
        where TRecord : IRecord, new()
    {
        var result = new TRecord[values.Length];
        var stacker = _sqlLanguage.InitQueryBuilder();
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
                if (val.TryGet(field.Name, out var v))
                {
                    if (!field.TryComputeValue(v, record, out var r)) throw new Exception(); //TODO
                    
                    fieldNames.Add(field.Name);
                    record.Init(field.Name, r);
                    parameters.AddRange(r!);
                }
                else if (field.Options is { Required: true, AutoIncrement: false }) throw new Exception(); //TODO maybe not exception
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

    internal List<T> SelectRecords<T>(IModel model, QueryCondition? where)
        where T : IRecord, new()
    {
        var mf = new SelectTree();
        mf.AddModelFields(model, ModelReference.None, model.AllFieldDefinitions.ToArray());
        return SelectRecords<T>(mf, where);
    }

    internal List<T> SelectRecords<T>(IModel model, IReadOnlyList<string> fields, QueryCondition? where)
        where T : IRecord, new()
    {
        var mf = new SelectTree();
        mf.AddFromStrings(model, _modelBank, fields);
        return SelectRecords<T>(mf, where);
    }

    private List<T> SelectRecords<T>(SelectTree tree, QueryCondition? where)
        where T : IRecord, new()
    {
        WhereSpecification? whereSpecification;
        IReadOnlyList<object> parameters;
        List<string> tempTables = [];
        if (where is null)
        {
            whereSpecification = null;
            parameters = [];
        } else (whereSpecification, parameters) = where.Compile();

        var stacker = _sqlLanguage.InitQueryBuilder();
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
                var subQueryBuilder = _sqlLanguage.InitQueryBuilder();
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
    
    public IModel? GetModel(string name) => _modelBank.GetModel(name);
    
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

public record RecordChange(IRecord Record, HashSet<string> Fields);

public class DirtyCollection : Dictionary<string, Dictionary<object, RecordChange>>
{
    public void AddToDirty(IModel model, IRecord record, string field)
    {
        if (!TryGetValue(model.Name, out var dic))
        {
            dic = new Dictionary<object, RecordChange>();
            this[model.Name] = dic;
        }

        if (!record.TryGet(model.GetPrimaryKey().Name, out var pk) || pk is null)
            throw new Exception("No primary key");
        
        if(!dic.TryGetValue(pk, out var change))
        {
            change = new RecordChange(record, new HashSet<string>());
            dic[pk] = change;
        }

        change.Fields.Add(field);
    }
}

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