using Core.Abstract;

namespace Core.Databases;

public abstract class Database : AbstractDatabase
{
    protected override AbstractModel? GetModel<TModel>()
    {
        return null;
    }

    protected override AbstractModel? GetModel(string name)
    {
        return null;
    }
}