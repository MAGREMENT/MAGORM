using System.Text;
using ORM.Abstract;
using ORM.Queries;
using ORM.Queries.Common;

namespace ORM.Languages.SQLite;

public class SqLiteLanguage : BaseSqlLanguage
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

    protected override string TemporaryAttribute => "TEMP";

    public override bool IsSameDBFieldType(DBFieldType left, DBFieldType right)
    {
        if ((left == DBFieldType.BOOL && right == DBFieldType.INT)
            || (left == DBFieldType.INT && right == DBFieldType.BOOL)) return true;
        
        return base.IsSameDBFieldType(left, right);
    }

    public override IQueryBuilder InitQueryBuilder() => new DividedQueriesBuilder(this);

    protected override void AddInsertFieldReturns(StringBuilder builder, ref int paramCount, InsertSpecification specification)
    {
        if (specification.ReturnedFields.Count == 0) return;
        
        builder.Append("\nRETURNING ");
        for (int i = 0; i < specification.ReturnedFields.Count; i++)
        {
            if (i > 0) builder.Append(", ");
            builder.Append(specification.ReturnedFields[i]);
        }
    }

    protected override string AutoIncrementAttribute => "AUTOINCREMENT";
    
    protected override bool PrimaryAutoIncrementByDefault => true;
}