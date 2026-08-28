using ORM.Abstract;
using ORM.Queries.Builder;
using ORM.Queries.Common;

namespace ORM.Languages.MySQL;

public class MySqlLanguage : BaseSqlLanguage
{
    public override IScriptBuilder InitScriptBuilder() => new StackingScriptBuilder();

    protected override string TranslateDBFieldType(DBFieldType type)
    {
        if (type == DBFieldType.STRING) return "VARCHAR(MAX)"; //TODO handle not max

        return type.ToString();
    }
    
    protected override string TemporaryAttribute => "TEMPORARY";

    protected override string AutoIncrementAttribute => "AUTO_INCREMENT";

    protected override bool PrimaryAutoIncrementByDefault => true;
}