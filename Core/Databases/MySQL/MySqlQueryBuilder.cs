using Core.Abstract;

namespace Core.Databases.MySQL;

public class MySqlQueryBuilder : BaseSqlQueryBuilder
{
    protected override string TranslateDBFieldType(DBFieldType type)
    {
        if (type == DBFieldType.STRING) return "VARCHAR(MAX)"; //TODO handle not max

        return type.ToString();
    }

    protected override string AutoIncrementAttribute => "AUTO_INCREMENT";

    protected override bool PrimaryAutoIncrementByDefault => true;
}