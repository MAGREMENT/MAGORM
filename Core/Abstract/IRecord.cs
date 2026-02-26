namespace Core.Abstract;

public abstract class Record : IRecord
{
    protected Database? _database;
    
    public virtual void AttachToDatabase(Database database)
    {
        _database = database;
    }
    
    public void SetFieldValue(string name, object value)
    {
        if(_database is null) throw new Exception();//TODO
            
        if(HasValue(name) && !GetFieldValue(name).Equals(value)) _database.NoticeDirty(this, name);
        
        InternalSetFieldValue(name, value);
    }

    protected abstract void InternalSetFieldValue(string name, object value);

    protected abstract bool HasValue(string name);

    public abstract object GetFieldValue(string name);
}

public interface IRecord : IDatabaseAttachable
{
    public void SetFieldValue(string name, object value);

    public object GetFieldValue(string name);
}