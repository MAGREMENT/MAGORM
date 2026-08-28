using Base.Fields;

namespace ORM.Queries;

public interface IQueryResult : IReadOnlyKeyValue<string, object?>, IReadOnlyKeyValue<int, object?>, IDisposable
{
    public bool Next();
    public bool Reset();
    public bool NextResultSet();
    
    public bool TryGetNullableInt(string name, out int? value);
    public bool TryGetInt(string name, out int value);
    
    public bool TryGetNullableString(string name, out string? value);
    public bool TryGetString(string name, out string value);
    
    public bool TryGetNullableBool(string name, out bool? value);
    public bool TryGetBool(string name, out bool value);
}

public interface IBufferedQueryResult : IQueryResult
{
    public void AddRow();

    public void AddColumn(string name, object? value);
}