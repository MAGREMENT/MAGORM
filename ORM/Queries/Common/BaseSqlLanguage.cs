using System.Text;
using ORM.Abstract;
using ORM.Queries.Specifications;

namespace ORM.Queries.Common;

public abstract class BaseSqlLanguage : ISqlLanguage
{
    public void Create(StringBuilder builder, ref int paramCount, CreateSpecification specification)
    {
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
                if (specification.PrimaryKey.Names.Count == 1)
                {
                    builder.Append(" PRIMARY KEY");
                    pkDone = true;
                }

                if (PrimaryAutoIncrementByDefault) ignoreAutoIncrement = true;
            }

            if (!ignoreAutoIncrement && f.AutoIncrement)
            {
                builder.Append(' ');
                builder.Append(AutoIncrementAttribute);
            }
            builder.Append(",\n");
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

        if(builder[^1] == '\n' && builder[^2] == ',') builder.Remove(builder.Length - 2, 2);
        builder.Append(')');
    }

    public void CreateFromSelect(StringBuilder builder, ref int paramCount, CreateFromSelectSpecification specification)
    {
        builder.Append("CREATE ");
        if (specification.IsTemporary)
        {
            builder.Append(TemporaryAttribute);
            builder.Append(' ');
        }
        builder.Append("TABLE ");
        builder.Append(specification.Name);
        builder.Append(" AS\n");
        Select(builder, ref paramCount, specification.Select);
    }

    public void Insert(StringBuilder builder, ref int paramCount, InsertSpecification specification)
    {
        builder.Append($"INSERT INTO {specification.Model} (");
        for (int i = 0; i < specification.Fields.Count; i++)
        {
            if (i > 0) builder.Append(", ");
            builder.Append(specification.Fields[i]);
        }

        builder.Append(")\nVALUES (");
        for (int i = 0; i < specification.Fields.Count; i++)
        {
            if (i > 0) builder.Append(", ");
            builder.Append(GetParameter(paramCount++));
        }

        builder.Append(')');
        AddInsertFieldReturns(builder, ref paramCount, specification);
    }

    protected virtual void AddInsertFieldReturns(StringBuilder builder, ref int paramCount,
        InsertSpecification specification) {}

    public void Update(StringBuilder builder, ref int paramCount, UpdateSpecification specification)
    {
        builder.Append($"UPDATE {specification.Model}\nSET");

        for (int i = 0; i < specification.Fields.Count; i++)
        {
            builder.Append(' ');
            if (i > 0) builder.Append(", ");
            builder.Append(specification.Fields[i]);
            builder.Append(" = ");
            builder.Append(GetParameter(paramCount++));
        }

        if (specification.Where is not null) Where(builder, ref paramCount, specification.Where);
    }

    public void Select(StringBuilder builder, ref int paramCount, SelectSpecification specification)
    {
        builder.Append("SELECT ");
        
        var isFirst = true;
        foreach (var field in specification.Fields)
        {
            if (isFirst) isFirst = false;
            else builder.Append(", ");
                
            builder.Append(field);
        }

        builder.Append($"\nFROM {specification.Model}");

        if (specification.Where is not null) Where(builder, ref paramCount, specification.Where);
        if (specification.OrderBy is not null && specification.OrderBy.Length > 0)
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
    }

    public void Drop(StringBuilder builder, ref int paramCount, string name)
    {
        builder.Append("DROP TABLE ");
        builder.Append(name);
    }

    public virtual bool IsSameDBFieldType(DBFieldType left, DBFieldType right)
    {
        return left == right;
    }

    public abstract IQueryBuilder InitQueryBuilder();

    private void Where(StringBuilder builder, ref int paramCount, WhereSpecification specification)
    {
        builder.Append("\nWHERE");
        AddWhere(builder, ref paramCount, specification);
    }

    //TODO limit parenthesis more
    private void AddWhere(StringBuilder builder, ref int paramCount, WhereSpecification specification)
    {
        builder.Append(' ');
        if(specification.Left.Type == WhereArgumentType.ARGUMENT) builder.Append('(');
        WhereArgument(builder, ref paramCount, specification.Left);
        if(specification.Left.Type == WhereArgumentType.ARGUMENT) builder.Append(')');
        builder.Append(' ');
        builder.Append(GetOperator(specification.Operator));
        builder.Append(' ');
        if(specification.Right.Type == WhereArgumentType.ARGUMENT) builder.Append('(');
        WhereArgument(builder, ref paramCount, specification.Right);
        if(specification.Right.Type == WhereArgumentType.ARGUMENT) builder.Append(')');
    }

    private void WhereArgument(StringBuilder builder, ref int paramCount, WhereArgument argument)
    {
        switch (argument.Type)
        {
            case WhereArgumentType.FIELD :
                builder.Append(argument.Text);
                break;
            case WhereArgumentType.ARGUMENT :
                AddWhere(builder, ref paramCount, argument.Argument);
                break;
            case WhereArgumentType.PARAMETER :
                builder.Append(GetParameter(paramCount++));
                break;
        }
    }

    protected virtual string GetParameter(int paramCount)
    {
        return "@" + paramCount;
    }

    protected string GetOperator(DBOperator @operator)
    {
        return @operator switch
        {
            DBOperator.NONE => string.Empty,
            DBOperator.PLUS => "+",
            DBOperator.MINUS => "-",
            DBOperator.OR => "OR",
            DBOperator.AND => "AND",
            DBOperator.LIKE => "LIKE",
            DBOperator.EQUAL => "=",
            DBOperator.IS => "IS",
            DBOperator.IN => "IN",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    protected abstract string TranslateDBFieldType(DBFieldType type);

    protected abstract string TemporaryAttribute { get; }
    
    protected abstract string AutoIncrementAttribute { get; }
    
    protected abstract bool PrimaryAutoIncrementByDefault { get; }
}