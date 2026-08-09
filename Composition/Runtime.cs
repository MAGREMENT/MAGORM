using API;
using Base;
using Base.Dependency;
using ORM.Abstract;

namespace Composition;

public class Runtime
{
    private readonly IModuleRegistry _registry;
    private readonly Database _database;
    private readonly IApi _api;
    private readonly IStaticFileMapper _staticFileMapper;

    private readonly List<Module> _modules = new();
    private bool _started;

    internal Runtime(IModuleRegistry registry, Database database, IApi api, IStaticFileMapper staticFileMapper)
    {
        _registry = registry;
        _database = database;
        _staticFileMapper = staticFileMapper;
        _api = api;
    }

    public void AddModule(Module module) => AddModules(module);
    
    public void AddModules(params Module[] modules) => AddModules((IEnumerable<Module>)modules);
    
    public void AddModules(IEnumerable<Module> modules)
    {
        Queue<Module> queue = new();
        ModuleList list = new();
        foreach (var module in modules)
        {
            queue.Enqueue(module);
            list.Add(module);
        }
        
        while (queue.Count > 0)
        {
            var curr = queue.Dequeue();
            foreach (var dependency in curr.Dependencies)
            {
                if (_modules.ContainsNamed(dependency) || list.ContainsNamed(dependency)) continue;

                var instance = _registry.InstantiateModule(dependency);
                if (instance is null) throw new Exception("Module does not exists : " + dependency);

                queue.Enqueue(instance);
                list.Add(instance);
            }
        }

        AddModulesInOrder(DependencyResolutionAlgorithms.Best(list));
    }

    private void AddModulesInOrder(IEnumerable<Module> modules)
    {
        _modules.AddRange(modules);
        foreach (var module in modules)
        {
            _database.AddModels(module.Models);
            foreach(var path in module.StaticFiles) _staticFileMapper.MapStaticFilesDirectory(path, path, false);
            foreach (var endpointProvide in module.Endpoints) endpointProvide.DefineEndpoints(_api);
        }

        if (_started)
        {
            _database.Sync();
        }
    }

    public void Start()
    {
        _database.Sync();
        _api.UseStaticFiles(_staticFileMapper);
        _api.Start();
        _started = true;
    }
}

public class ModuleList : List<Module>, IDependencyCollection<Module>
{
    public IEnumerable<Module> Enumerate() => this;

    public IEnumerable<string> GetDependsOn(Module named) => named.Dependencies;

    public int GetDependsOnCount(Module named) => named.Dependencies.Count;
}