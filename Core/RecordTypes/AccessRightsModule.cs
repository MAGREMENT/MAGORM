using Base.PropertyCollections;
using Core.Abstract;

namespace Core.RecordTypes;

public class AccessRightsModule(IAccessContext _accessContext) : RecordModule
{
    public override void BeforeGetValue(IPropertyCollection context, string name)
    {
        if (false /*TODO*/) throw new Exception();
        base.BeforeGetValue(context, name);
    }

    public override void BeforeSetValue(IPropertyCollection context, string name, object? value)
    {
        if (false /*TODO*/) throw new Exception();
        base.BeforeSetValue(context, name, value);
    }
}

public interface IAccessContext
{
    
}