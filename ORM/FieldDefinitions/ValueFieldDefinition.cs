using ORM.Abstract;

namespace ORM.FieldDefinitions;

public abstract class ValueFieldDefinition(string name, FieldDefinitionsOptions options) : FieldDefinition
{
    public override string Name { get; } = name;
    public override FieldDefinitionsOptions Options { get; } = options;
    public abstract override DBFieldType GetDBFieldType();
    public override ModelReference? Reference => null;

    public override object? ToDbValue(object? recordValue) => recordValue;

    public override void Attach(IReadOnlyModelRegistry obj) { }

    public override void Detach(IReadOnlyModelRegistry obj) { }
}