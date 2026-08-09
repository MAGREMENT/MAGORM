using ORM.Languages.MySQL;
using ORM.Languages.SQLite;
using ORM.Queries;

namespace ORM.Languages;

public static class SQL
{
    public static readonly ISqlLanguage SqLite = new SqLiteLanguage();
    public static readonly MySqlLanguage MySql = new MySqlLanguage();
}