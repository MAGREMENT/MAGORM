namespace Core.Abstract;

public interface IQueryMaker
{
    protected string GenerateCreationScript(ModelSpecification model);
}