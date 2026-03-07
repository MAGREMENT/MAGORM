namespace Core.Abstract;

public abstract class TrackedRecord : IRecord
{
    private Database? _database;

    public virtual void AttachToDatabase(Database database)
    {
        _database = database;
    }

    public void SetValue(string name, object? value, bool ignoreOnSet = false)
    {
        if (!ignoreOnSet)
        {
            var valueBefore = TryGetValue(name, out var v) ? v : null; //TODO manage case where no value is set
            BeforeOnSet(name, valueBefore, value);
        }
        
        InternalSetValue(name, value);
    }

    protected void BeforeOnSet(string name, object? valueBefore, object? value)
    {
        if(_database is null) throw new Exception();//TODO

        bool shouldNotice;
        if (valueBefore is null) shouldNotice = value is not null;
        else shouldNotice = !valueBefore.Equals(value);
                
        if(shouldNotice) _database.NoticeDirty(this, name);
    }

    protected abstract void InternalSetValue(string name, object? value);

    public abstract int GetFieldCount();

    public abstract IEnumerable<string> GetFieldNames();

    public abstract bool TryGetValue(string key, out object? value);
}

public interface IRecord : IDatabaseAttachable, IKeyValue<string, object?>
{
    public int GetFieldCount();

    public IEnumerable<string> GetFieldNames();
    
    public void SetValue(string name, object? value, bool ignoreOnSet = false);
}

public static class RecordExtensions
{
    public static bool IsFullyEqualTo(this IRecord record1, IRecord record2)
    {
        if (record1.GetFieldCount() != record2.GetFieldCount()) return false;

        return record1.AreCommonFieldsEqual(record2);
    }
    
    public static bool AreCommonFieldsEqual(this IRecord record1, IRecord record2)
    {
        foreach (var name in record1.GetFieldNames())
        {
            if (!record1.TryGetValue(name, out var value1))
                throw new Exception("Something terribly wrong has happened");

            if (!record2.TryGetValue(name, out var value2)) continue;

            if (value1 is null)
            {
                if (value2 is not null) return false;
            }
            else if (!value1.Equals(value2)) return false;
        }

        return true;
    }
}