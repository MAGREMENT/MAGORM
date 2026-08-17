using ORM.ModelTypes;

namespace ORM.Abstract;

public static class DatabaseExtensions
{
    public static TRecord InsertRecord<TRecord>(this Database database, IModel model, TRecord record)
        where TRecord : IRecord
    {
        return database.InsertRecords(model, [record]).First();
    }
}