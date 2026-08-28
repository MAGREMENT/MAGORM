using ORM.Abstract;
using ORM.Queries;

namespace ORM.FieldDefinitions.ValueTypes;

public class StringValueFieldDefinition(string name, FieldDefinitionsOptions options) 
    : ValueFieldDefinition(name, options)
{
    public override DBFieldType GetDBFieldType() => DBFieldType.STRING;
    
    public override bool TryFetchFromQueryResult(IQueryResult result, string name, out object? value)
    {
        bool success;
        if (Options.Required)
        {
            success = result.TryGetString(name, out var v);
            value = v;
        }
        else
        {
            success = result.TryGetNullableString(name, out var nv);
            value = nv;
        }
        return success;
    }
}