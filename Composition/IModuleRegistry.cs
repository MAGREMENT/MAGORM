namespace Composition;

public interface IModuleRegistry
{
    Module? InstantiateModule(string s);
}

//TODO JSON Registry
public class DictionaryModuleRegistry : Dictionary<string, Module>, IModuleRegistry
{
    public Module? InstantiateModule(string s) => this.GetValueOrDefault(s);
}