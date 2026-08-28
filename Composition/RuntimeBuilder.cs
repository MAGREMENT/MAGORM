using API;
using API.StaticFiles;
using ORM.Abstract;
using ORM.Languages;
using ORM.Queries;

namespace Composition;

public class RuntimeBuilder
{
    private IDatabaseEngine? _engine;
    private IApi? _api;
    private IStaticFileMapper _mapper = new InMemoryStaticFileMapper();
    
    private RuntimeBuilder() {}

    public static RuntimeBuilder Start() => new();

    public void UseDatabaseEngine(IDatabaseEngine engine) => _engine = engine;

    public void UseApi(IApi api) => _api = api;

    public void UseStaticFileMapper(IStaticFileMapper mapper) => _mapper = mapper;

    public Runtime Build()
    {
        if (_engine is null) throw new Exception("A database engine is needed");
        if (_api is null) throw new Exception("An API is needed");

        return new Runtime(
            new DictionaryModuleRegistry(),
            new Database(_engine, new DictionaryModelRegistry()), 
            _api,
            _mapper);
    }
}