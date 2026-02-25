namespace Core.Abstract;

public abstract class TrackableRecord : Record
{
    internal bool IsTracked { get; set; } = false;
    
    public override void SetFieldValue(string name, object value)
    {
        if (IsTracked)
        {
            if(_database is null) throw new Exception();//TODO
            
            if(!GetFieldValue(name).Equals(value)) _database.NoticeDirty(null! /*TODO figure out of to give model */,
                this, name);
        }
        
        base.SetFieldValue(name, value);
    }

    public void Untrack()
    {
        IsTracked = false;
    }
}

public abstract class Record : IRecord
{
    protected Database? _database;
    
    public virtual void SetFieldValue(string name, object value)
    {
        
    }
    
    public virtual void AttachToDatabase(Database database)
    {
        _database = database;
    }
    
    protected abstract void InternalSetFieldValue(string name, object value);
    
    public abstract object GetFieldValue(string name);
}

public interface IRecord : IDatabaseAttachable
{
    public void SetFieldValue(string name, object value);

    public object GetFieldValue(string name);
}