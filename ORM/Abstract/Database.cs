using ORM.ModelTypes;
using ORM.Queries;
using ORM.Queries.Specifications;

namespace ORM.Abstract;

public class Database
{
    private readonly IDatabaseEngine _engine;
    private readonly ModelRegistry _modelRegistry;

    public Database(IDatabaseEngine engine, ModelRegistry modelRegistry)
    {
        _engine = engine;
        _modelRegistry = modelRegistry;
    }

    public void AddModels(params IModel[] models) => AddModels((IEnumerable<IModel>) models);

    public void AddModels(IEnumerable<IModel> models)
    {
        _modelRegistry.AddModels(models);
    }

    public IEnumerable<IModel> EnumerateModels() => _modelRegistry.EnumerateModels();
    
    public IModel? GetModel(string name) => _modelRegistry.GetModel(name);
    
    public void Sync()
    {
        var currentSpecifications = new Dictionary<string, CreateSpecification>();
        
        foreach (var s in _engine.GetModelSchemas())
        {
            currentSpecifications.Add(s.Model, s);
        }

        var builder = _engine.Language.InitScriptBuilder();
        foreach (var desiredModel in _modelRegistry.EnumerateModels())
        {
            var desiredSpecification = desiredModel.GenerateCreateSpecification();
            if (!currentSpecifications.TryGetValue(desiredSpecification.Model, out var currSpecification))
            {
                _engine.Language.Create(builder, desiredSpecification);
            }
            else
            {
                //TODO update columns
            }
        }

        var transaction = _engine.CreateTransaction();
        transaction.Execute(builder);
        transaction.Commit();
    }

    public void Nuke()
    {
        //TODO do something else like : check if language has a drop all tables cmd. If no, get schema and drop tables one by one
        _engine.DropAllTables();
    }

    public IEnumerable<TRecord> InsertRecords<TRecord>(IModel model, IEnumerable<TRecord> records) where TRecord : IRecord
    {
        var builder = _engine.Language.InitScriptBuilder();
        ModelHandling.InsertRecords(new ModelScriptBuildingContext(builder, _engine.Language), model, records);
        
        _engine.ExecuteResult(queryResult =>
        {
            using var enumerator = records.GetEnumerator();
            while(queryResult.Next())
            {
                enumerator.MoveNext();
                var record = enumerator.Current;
                
                foreach (var field in model.GetAllAutoIncrementFields())
                {
                    if (!field.TryFetchFromQueryResult(queryResult, field.Name, out var value)) throw new Exception(); //TODO
                    record.Init(field.Name, value);
                }
            }

            return true;
        }, builder);
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
        mf.AddFromStrings(model, fields);
        return SelectRecords<T>(mf, where);
    }

    public void UpdateRecords(IEnumerable<RecordUpdate> changes)
    {
        var builder = _engine.Language.InitScriptBuilder();
        ModelHandling.UpdateRecords(new ModelScriptBuildingContext(builder, _engine.Language),changes);

        _engine.Execute(builder);
    }

    public void DeleteRecords(IEnumerable<RecordDelete> deletes)
    {
        var builder = _engine.Language.InitScriptBuilder();
        ModelHandling.DeleteRecords(new ModelScriptBuildingContext(builder, _engine.Language),deletes);
        
        _engine.Execute(builder);
    }

    private List<T> SelectRecords<T>(SelectTree tree, QueryCondition? where)
        where T : IRecord, new()
    {
        var builder = _engine.Language.InitScriptBuilder();
        ModelHandling.SelectRecords(new ModelScriptBuildingContext(builder, _engine.Language), tree, where);
        
        return _engine.ExecuteResult<List<T>>(queryResult => CreateRecordsFromQueryResult<T>(queryResult, tree), 
            builder);
    }
    
    private static List<T> CreateRecordsFromQueryResult<T>(IQueryResult queryResult, SelectTree tree)
        where T : IRecord, new()
    {
        var i = 0;
        var result = new List<T>();
        // (Model name, Record Primary Key) (Needing Reference Record, Needing Reference Field)[]
        var needs = new Dictionary<(string, object?), List<(IRecord, string)>>();
        var modelsInOrder = tree.ModelsInDependencyOrder;
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
                    if (!definition.TryFetchFromQueryResult(queryResult, definition.Name, out var value)) throw new Exception(); //TODO
                    
                    if (definition.Reference is not null)
                    {
                        if (!needs.TryGetValue((definition.Reference.Model.Name, value), out needingReferenceList))
                        {
                            needingReferenceList = new List<(IRecord, string)>();
                            needs[(definition.Reference.Model.Name, value)] = needingReferenceList;
                        }
                        needingReferenceList.Add((record, definition.Name));
                    } else record.Init(definition.Name, value);
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

public class MissingFieldException(IFieldDefinition definition) 
    : Exception($"The field {definition.Name} is required and missing from the values");