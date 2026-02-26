namespace Core.Abstract;

public class Database
{
    private readonly IQueryBuilder _queryBuilder;
    private readonly IDatabaseEngine _engine;
    private readonly IModelBank _modelBank;
    private readonly DirtyCollection _dirty = new();

    public Database(IQueryBuilder queryBuilder, IDatabaseEngine engine, IModelBank modelBank)
    {
        _queryBuilder = queryBuilder;
        _engine = engine;
        _modelBank = modelBank;
    }

    public void AddModels(params Model[] models) => AddModels((IEnumerable<Model>) models);

    public void AddModels(IEnumerable<Model> models)
    {
        _modelBank.AddModels(models);
        foreach (var model in models)
        {
            model.AttachToDatabase(this);
        }
    }

    public void NoticeDirty(IRecord record, string field)
    {
        /*TODO figure out how to give model */
        _dirty.AddToDirty(null!, record, field);
    }

    public void Sync()
    {
        var currentSpecifications = new Dictionary<string, ModelSpecification>();
        
        foreach (var s in _engine.GetModelSchemas())
        {
            currentSpecifications.Add(s.Model, s);
        }

        var stacker = new QueryStacker();
        foreach (var desiredModel in _modelBank)
        {
            var desiredSpecification = desiredModel.GenerateSpecification();
            if (!currentSpecifications.TryGetValue(desiredSpecification.Model, out var currSpecification))
            {
                stacker.AddQuery(_queryBuilder.Create(desiredSpecification));
            }
            else
            {
                //TODO update columns
            }
        }

        var transaction = _engine.CreateTransaction();
        transaction.Execute(stacker.GetQuery(), stacker.GetParameters());
        transaction.Commit();
    }

    public void Nuke()
    {
        _engine.DropAllTables();
    }

    public TRecord[] CreateRecords<TRecord>(Model model, params Dictionary<string, object>[] values)
        where TRecord : IRecord, new()
    {
        if (GetModel(model.Name) is null) throw new Exception();//TODO
        
        var result = new TRecord[values.Length];
        var stacker = new QueryStacker();
        var fieldNames = new List<string>();
        
        for(int i = 0; i < values.Length; i++)
        {
            var val = values[i];

            var record = new TRecord();
            record.AttachToDatabase(this);
            result[i] = record;
            
            foreach (var field in model.AllFieldDefinitions)
            {
                if(!field.IsStored()) continue;
                
                if (val.TryGetValue(field.Name, out var v))
                {
                    if (!field.TryComputeValue(v, record, out var r)) throw new Exception(); //TODO
                    
                    fieldNames.Add(field.Name);
                    record.SetFieldValue(field.Name, r);
                    stacker.AddParameter(r);
                }
                else if (field.Options is { Required: true, AutoIncrement: false }) throw new Exception(); //TODO maybe not exception
            }
            
            stacker.AddQuery(_queryBuilder.Insert(new InsertSpecification(model.Name, fieldNames.ToArray())));
        }

        var transaction = _engine.CreateTransaction();
        transaction.Execute(stacker.GetQuery(), stacker.GetParameters());
        transaction.Commit();
        
        return result;
    }

    public Model? GetModel<TModel>() => _modelBank.GetModel<TModel>();
    public Model? GetModel(string name) => _modelBank.GetModel(name);
}

public interface IDatabaseAttachable
{
    void AttachToDatabase(Database database);
}

public record RecordChange(IRecord Record, HashSet<string> Fields);

public class DirtyCollection : Dictionary<string, Dictionary<object, RecordChange>>
{
    public void AddToDirty(Model model, IRecord record, string field)
    {
        if (!TryGetValue(model.Name, out var dic))
        {
            dic = new Dictionary<object, RecordChange>();
            this[model.Name] = dic;
        }

        var pk = record.GetFieldValue(model.GetPrimaryKey().Name);
        if(!dic.TryGetValue(pk, out var change))
        {
            change = new RecordChange(record, new HashSet<string>());
            dic[pk] = change;
        }

        change.Fields.Add(field);
    }
}