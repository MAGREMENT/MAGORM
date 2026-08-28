using ORM.Abstract;
using ORM.Queries;

namespace ORM.FieldDefinitions.ValueTypes;

public class BoolValueFieldDefinition(string name, FieldDefinitionsOptions options) 
    : ValueFieldDefinition(name, options)
{
    public override DBFieldType GetDBFieldType() => DBFieldType.BOOL;
    
    public override bool TryFetchFromQueryResult(IQueryResult result, string name, out object? value)
    {
        bool success;
        if (Options.Required)
        {
            success = result.TryGetBool(name, out var v);
            value = v;
        }
        else
        {
            success = result.TryGetNullableBool(name, out var nv);
            value = nv;
        }
        return success;
    }
}