namespace ORM.Abstract;

public interface IModelBank
{
    IModel? GetModel(string name);
    void AddModels(IEnumerable<IModel> models);
    IEnumerable<IModel> EnumerateModels();
}