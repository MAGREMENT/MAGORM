using Core.Abstract;

namespace Core.ModelTypes;

public class DictionaryModel : Model
{
    private readonly Dictionary<string, IFieldDefinition> _dic = new();
    private readonly IFieldDefinition _primaryKey;

    public DictionaryModel(string name, IFieldDefinition primaryKey, params IFieldDefinition[] fields)
    {
        Name = name;
        
        _primaryKey = primaryKey;
        _dic.Add(_primaryKey.Name, _primaryKey);
        
        foreach (var f in fields)
        {
            _dic.Add(f.Name, f);
        }
    }
    
    public override string Name { get; }

    public override IFieldDefinition GetPrimaryKey() => _primaryKey;

    public override IFieldDefinition? GetFieldDefinition(string name) => _dic.GetValueOrDefault(name, null!);

    public override IReadOnlyCollection<IFieldDefinition> AllFieldDefinitions => _dic.Values;
}