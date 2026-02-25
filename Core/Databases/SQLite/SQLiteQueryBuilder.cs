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

    public override bool IsSameDBFieldType(DBFieldType left, DBFieldType right)
    {
        if ((left == DBFieldType.BOOL && right == DBFieldType.INT)
            || (left == DBFieldType.INT && right == DBFieldType.BOOL)) return true;
        
        return base.IsSameDBFieldType(left, right);
    }

    protected override string AutoIncrementAttribute => "AUTOINCREMENT";
    
    protected override bool PrimaryAutoIncrementByDefault => true;
}