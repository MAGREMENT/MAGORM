namespace Core.Abstract;

public interface ITrackingContext
{
    public void AddToDirty(IRecord record);
}