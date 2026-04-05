using Base.PropertyCollections;
using Core.Abstract;

namespace Core.RecordTypes;

public class TrackedRecordModule(Model _model) : RecordModule
{ 
    public override void BeforeSetValue(IPropertyCollection context, string name, object? value)
    {
        _model.NoticeDirty((IRecord)context, name);
        base.BeforeSetValue(context, name, value);
    }
}