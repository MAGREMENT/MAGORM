using Core.Abstract;

namespace Core.Databases.SQLite;

public class SQLiteQueryBuilder : BaseSqlQueryBuilder
{
    protected override string TranslateDBFieldType(DBFieldType type)
    {
        return type switch
        {
            DBFieldType.INT => "INTEGER",
            DBFieldType.STRING => "TEXT",
            DBFieldType.BOOL => "INTEGER",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    protected override string AutoIncrementAttribute => "AUTOINCREMENT";
    
    protected override bool PrimaryAutoIncrementByDefault => true;
}