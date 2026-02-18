namespace Core.Abstract;

public abstract class TrackableRecord : IRecord
{
    internal bool IsTracked { get; set; } = false;

    internal ITrackingContext? Context { get; set; } = null;
    
    public void SetFieldValue(string name, object value)
    {
        if (IsTracked)
        {
            if (Context is null) throw new Exception();//TODO
            
            if(!GetFieldValue(name).Equals(value)) Context.AddToDirty(this);
        }
        
        InternalSetFieldValue(name, value);
    }

    public void Untrack()
    {
        IsTracked = false;
    }

    protected abstract void InternalSetFieldValue(string name, object value);

    public abstract object GetFieldValue(string name);
}

public interface IRecord
{
    public void SetFieldValue(string name, object value);

    public object GetFieldValue(string name);
}