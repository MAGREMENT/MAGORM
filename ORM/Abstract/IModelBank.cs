using ORM.ModelTypes;

namespace ORM.Abstract;

public interface IReadOnlyModelBank
{
    IModel? GetModel(string name);
    IEnumerable<IModel> EnumerateModels();
}

public interface IModelBank : IReadOnlyModelBank
{
    void AddModels(IEnumerable<IModel> models);
}