using ORM.Abstract;
using ORM.RecordTypes;

namespace ORM.ModelTypes;

public class DictionaryModel : Model
{
    private readonly Dictionary<string, IFieldDefinition> _dic = new();
    private readonly IFieldDefinition _primaryKey;

    public DictionaryModel(string name, IFieldDefinition primaryKey, params IFieldDefinition[] fields)
    {
        Name = name;
        
        _primaryKey = primaryKey;
        Add(primaryKey);
        
        foreach (var f in fields)
        {
            Add(f);
        }
    }

    public sealed override void Add(IFieldDefinition field)
    {
        base.Add(field);
        _dic.Add(field.Name, field);
    }

    public override string Name { get; }

    public override IFieldDefinition GetPrimaryKey() => _primaryKey;

    public override IFieldDefinition? GetFieldDefinition(string name) => _dic.GetValueOrDefault(name, null!);
    public override IFieldDefinition? GetFieldDefinition(ReadOnlySpan<char> name) => _dic.GetValueOrDefault(name.ToString(), null!);

    public override IReadOnlyCollection<IFieldDefinition> AllFieldDefinitions => _dic.Values;
    public override IRecord InstantiateRecord() => new DictionarySideEffectRecord(); //TODO change
}