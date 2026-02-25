using System.Text;
using Core.Abstract;

namespace Core.Databases;

public abstract class BaseSqlQueryBuilder : IQueryBuilder
{
    public Query Create(ModelSpecification specification)
    {
        var builder = new StringBuilder();

        builder.Append($"CREATE TABLE {specification.Model} (\n");

        var pkDone = false;
        for(int i = 0; i < specification.Fields.Count; i++)
        {
            var f = specification.Fields[i];
            builder.Append($"    {f.Name} {TranslateDBFieldType(f.FieldType)}");

            if (f.Unique) builder.Append(" UNIQUE");
            if (f.Required) builder.Append(" NOT NULL");

            var ignoreAutoIncrement = false;
            if (specification.PrimaryKey.Names.Contains(f.Name))
            {
                if (specification.PrimaryKey.Names.Length == 1)
                {
                    builder.Append(" PRIMARY KEY");
                    pkDone = true;
                }

                if (PrimaryAutoIncrementByDefault) ignoreAutoIncrement = true;
            }
            if (!ignoreAutoIncrement && f.AutoIncrement) builder.Append(" AUTO_INCREMENT");
            
            if(i < specification.Fields.Count - 1) builder.Append(",\n");
        }

        if (!pkDone)
        {
            builder.Append("    PRIMARY KEY (");
            foreach (var n in specification.PrimaryKey.Names)
            {
                builder.Append(' ');
                builder.Append(n);
            }
            builder.Append("),\n");
        }

        foreach (var fk in specification.ForeignKeys)
        {
            builder.Append($"    FOREIGN KEY ({fk.Field}) REFERENCES {fk.OtherModel}({fk.OtherField}),\n");
        }

        builder.Append(");");
        return new Query(builder.ToString(), 0);
    }

    public Query Insert(InsertSpecification specification)
    {
        var builder = new StringBuilder();

        builder.Append($"INSERT INTO {specification.Model} (");
        for (int i = 0; i < specification.Fields.Length; i++)
        {
            if (i > 0) builder.Append(", ");
            builder.Append(specification.Fields[i]);
        }

        builder.Append(")\nVALUES (");
        for (int i = 0; i < specification.Fields.Length; i++)
        {
            if (i > 0) builder.Append(", ");
            builder.Append('@' + i);
        }

        builder.Append(");");

        return new Query(builder.ToString(), specification.Fields.Length);
    }

    public Query Update(UpdateSpecification specification)
    {
        var builder = new StringBuilder();

        builder.Append($"UPDATE {specification.Model}\nSET");

        for (int i = 0; i < specification.Fields.Length; i++)
        {
            builder.Append(' ');
            if (i > 0) builder.Append(", ");
            builder.Append(specification.Fields[i]);
            builder.Append(" = ");
            builder.Append('@' + i);
        }

        var whereQuery = Where(specification.Where, specification.Fields.Length);
        builder.Append(whereQuery);
        builder.Append(';');
        
        return new Query(builder.ToString(), specification.Fields.Length + whereQuery.ParameterCount);
    }

    public Query Select(SelectSpecification specification)
    {
        var builder = new StringBuilder();

        builder.Append("SELECT ");

        if (specification.Fields.Length == 0) builder.Append('*');
        else
        {
            for (int i = 0; i < specification.Fields.Length; i++)
            {
                if (i > 0) builder.Append(", ");
                builder.Append(specification.Fields[i]);
            }
        }

        builder.Append($"\nFROM {specification.Model}");
        
        var whereQuery = Where(specification.Where, specification.Fields.Length);
        builder.Append(whereQuery);

        if (specification.OrderBy.Length > 0)
        {
            builder.Append("\nORDER BY");
            for (int i = 0; i < specification.OrderBy.Length; i++)
            {
                builder.Append(' ');
                if (i > 0) builder.Append(", ");
                builder.Append(specification.OrderBy[i].Field);
                builder.Append(' ');
                builder.Append(specification.OrderBy[i].Type);
            }
        }
        
        builder.Append(';');
        
        return new Query(builder.ToString(), specification.Fields.Length + whereQuery.ParameterCount);
    }

    public virtual bool IsSameDBFieldType(DBFieldType left, DBFieldType right)
    {
        return left == right;
    }

    private Query Where(WhereSpecification specification, int currParameterCount)
    {
        return new Query(string.Empty, 0); //TODO
    }

    protected abstract string TranslateDBFieldType(DBFieldType type);

    protected abstract string AutoIncrementAttribute { get; }
    
    protected abstract bool PrimaryAutoIncrementByDefault { get; }
}