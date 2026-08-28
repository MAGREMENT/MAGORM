using ORM.Abstract;
using ORM.Queries;

namespace ORM.FieldDefinitions.ValueTypes;

public class IntValueFieldDefinition(string name, FieldDefinitionsOptions options) 
    : ValueFieldDefinition(name, options)
{
    public override DBFieldType GetDBFieldType() => DBFieldType.INT;
    
    public override bool TryFetchFromQueryResult(IQueryResult result, string name, out object? value)
    {
        bool success;
        if (Options.Required)
        {
            success = result.TryGetInt(name, out var v);
            value = v;
        }
        else
        {
            success = result.TryGetNullableInt(name, out var nv);
            value = nv;
        }
        return success;
    }
}