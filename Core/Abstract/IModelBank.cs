namespace Core.Abstract;

public interface IModelBank : IEnumerable<Model>
{
    Model? GetModel<TModel>();
    Model? GetModel(string name);

    void AddModels(IEnumerable<Model> models);
}