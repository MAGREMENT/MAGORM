using System.Text;
using Core.Abstract;

namespace Core.Databases.MySQL;

public class MySQLQueryMaker : IQueryMaker
{
    public string GenerateCreationScript(ModelSpecification model)
    {
        var builder = new StringBuilder();

        builder.Append($"CREATE TABLE {model.Name} (\n");

        foreach (var f in model.Fields)
        {
            builder.Append($"    {f.Name} {ToString(f.FieldType)}");

            if (f.Unique) builder.Append(" UNIQUE");
            if (f.Required) builder.Append(" NOT NULL");
            if (f.AutoIncrement) builder.Append(" AUTO_INCREMENT");
            
            builder.Append(",\n");
        }

        builder.Append("    PRIMARY KEY (");
        foreach (var n in model.PrimaryKey.Names)
        {
            builder.Append(' ');
            builder.Append(n);
        }
        builder.Append("),\n");

        foreach (var fk in model.ForeignKeys)
        {
            builder.Append($"    FOREIGN KEY ({fk.Field}) REFERENCES {fk.OtherModel}({fk.OtherField}),\n");
        }

        builder.Append(");");
        return builder.ToString();
    }

    public string ToString(DBFieldType type)
    {
        if (type == DBFieldType.VARCHAR) return "VARCHAR(MAX)"; //TODO handle not max

        return type.ToString();
    }
}