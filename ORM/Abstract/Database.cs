using Base;
using Base.Dependency;
using Base.Fields;
using ORM.ModelTypes;
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
            model.Attach(_modelBank);
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
        //TODO do something else like : check if language has a drop all tables cmd. If no, get schema and drop tables one by one
        _engine.DropAllTables();
    }

    public IEnumerable<TRecord> InsertRecords<TRecord>(IModel model, IEnumerable<TRecord> records) where TRecord : IRecord
    {
        var stacker = _engine.Language.InitQueryBuilder();
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
            
            stacker.Insert(new InsertSpecification(model.Name, fieldNames.ToArray(),
                model.GetAllAutoIncrementFieldsName()), parameters.ToArray());
        }
        
        _engine.ExecuteResult(queryResult =>
        {
            using var enumerator = records.GetEnumerator();
            while(queryResult.Next())
            {
                enumerator.MoveNext();
                var record = enumerator.Current;
                
                foreach (var field in model.GetAllAutoIncrementFields())
                {
                    var objValue = queryResult.TryGet(field.Name, out var v) ? v : null;
                    if (!field.TryComputeValue(objValue, record, out var value)) throw new Exception(); //TODO

                    record.Init(field.Name, value);
                }
            }

            return true;
        }, stacker.ToQueries());
        return records;
    }

    public List<T> SelectRecords<T>(IModel model, QueryCondition? where)
        where T : IRecord, new()
    {
        var mf = new SelectTree();
        mf.AddModelFields(model, model.AllFieldDefinitions.ToArray());
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
        List<object?> parameters;
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
            if (info.DependsOn.Count == 0)
            {
                var tempName = model.Name + "_results";
                stacker.CreateFromSelect(new CreateFromSelectSpecification(tempName, 
                    new SelectSpecification(model.Name, info.Fields, whereSpecification,
                    []), true), parameters);
                stacker.Select(new SelectSpecification(tempName, ISelectFieldSpecification.Star));
                tempTables.Add(tempName);
            }
            else
            {
                var modelConditions = new QueryCondition[info.DependsOn.Count];
                var pkName = model.GetPrimaryKey().Name; //TODO need to handle situation where pk is not the referenced field
                var i = 0;
                var subQueryBuilder = _engine.Language.InitQueryBuilder();
                foreach (var modelRef in info.DependsOn)
                {
                    subQueryBuilder.Reset();
                    subQueryBuilder.Select(new SelectSpecification(modelRef.Model + "_results", [modelRef.Field]));
                    modelConditions[i++] = new QueryCondition(pkName, DBOperator.IN, subQueryBuilder.ToQuery());
                }
                
                stacker.Select(new SelectSpecification(model.Name, info.Fields, 
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
                    if (definition.Reference is not null)
                    {
                        if (!needs.TryGetValue((definition.Reference.Model.Name, value), out needingReferenceList))
                        {
                            needingReferenceList = new List<(IRecord, string)>();
                            needs[(definition.Reference.Model.Name, value)] = needingReferenceList;
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

            if (i == 0 && result.Count == 0) return result;
            
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
    public readonly HashSet<ModelReference> DependsOn = [];
    public readonly HashSet<IFieldDefinition> Fields = [];
}

public class SelectTree : Dictionary<IModel, SelectTreeModelInfo>, IDependencyCollection<IModel>
{
    public IModel? AddModelField(IModel model, IFieldDefinition field)
    {
        var info = GetInfo(model);
        info.Fields.Add(field);
        if (field.Reference is not null)
        {
            var referenceInfo = GetInfo(field.Reference.Model);
            referenceInfo.DependsOn.Add(new ModelReference(model, field));
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
                referenceInfo.Fields.Add(field.Reference.Field);
            }
        }
    }

    public void AddFromStrings(IModel startModel, IModelBank bank, IReadOnlyList<string> strings)
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

public class MissingFieldException(IFieldDefinition definition) 
    : Exception($"The field {definition.Name} is required and missing from the values");