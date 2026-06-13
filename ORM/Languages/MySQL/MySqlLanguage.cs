using ORM.Abstract;
using ORM.Queries;
using ORM.Queries.Common;

namespace ORM.Languages.MySQL;

public class MySqlLanguage : BaseSqlLanguage
{
    public override IQueryBuilder InitQueryBuilder() => new StackingQueryBuilder(this);

    protected override string TranslateDBFieldType(DBFieldType type)
    {
        if (type == DBFieldType.STRING) return "VARCHAR(MAX)"; //TODO handle not max

        return type.ToString();
    }
    
    protected override string TemporaryAttribute => "TEMPORARY";

    protected override string AutoIncrementAttribute => "AUTO_INCREMENT";

    protected override bool PrimaryAutoIncrementByDefault => true;
}