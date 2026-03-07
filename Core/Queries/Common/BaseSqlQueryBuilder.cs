using System.Text;
using Core.Abstract;
using Core.Queries.Specifications;

namespace Core.Queries.Common;

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
                if (specification.PrimaryKey.Names.Count == 1)
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
        for (int i = 0; i < specification.Fields.Count; i++)
        {
            if (i > 0) builder.Append(", ");
            builder.Append(specification.Fields[i]);
        }

        builder.Append(")\nVALUES (");
        for (int i = 0; i < specification.Fields.Count; i++)
        {
            if (i > 0) builder.Append(", ");
            builder.Append(GetParameter(i));
        }

        builder.Append(");");

        return new Query(builder.ToString(), specification.Fields.Count);
    }

    public Query Update(UpdateSpecification specification)
    {
        var builder = new StringBuilder();
        var additionalParameterCount = 0;

        builder.Append($"UPDATE {specification.Model}\nSET");

        for (int i = 0; i < specification.Fields.Count; i++)
        {
            builder.Append(' ');
            if (i > 0) builder.Append(", ");
            builder.Append(specification.Fields[i]);
            builder.Append(" = ");
            builder.Append(GetParameter(i));
        }

        if (specification.Where is not null)
        {
            var whereQuery = Where(specification.Where, specification.Fields.Count);
            builder.Append(whereQuery);
            additionalParameterCount += whereQuery.ParameterCount;
        }
        
        builder.Append(';');
        
        return new Query(builder.ToString(), specification.Fields.Count + additionalParameterCount);
    }

    public Query Select(SelectSpecification specification)
    {
        var builder = new StringBuilder();
        var parameterCount = 0;

        builder.Append("SELECT ");

        if (specification.Fields.Count == 0) builder.Append('*');
        else
        {
            for (int i = 0; i < specification.Fields.Count; i++)
            {
                if (i > 0) builder.Append(", ");
                builder.Append(specification.Fields[i]);
            }
        }

        builder.Append($"\nFROM {specification.Model}");

        if (specification.Where is not null)
        {
            var whereQuery = Where(specification.Where, specification.Fields.Count);
            builder.Append(whereQuery);
            parameterCount += whereQuery.ParameterCount;
        }

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
        
        return new Query(builder.ToString(), parameterCount);
    }

    public virtual bool IsSameDBFieldType(DBFieldType left, DBFieldType right)
    {
        return left == right;
    }

    private Query Where(WhereSpecification specification, int currParameterCount)
    {
        var builder = new StringBuilder();
        Where(specification, ref currParameterCount, builder);
        return new Query(builder.ToString(), currParameterCount);
    }

    private void Where(WhereSpecification specification, ref int currParameterCount, StringBuilder builder)
    {
        builder.Append('(');
        WhereArgument(specification.Left, ref currParameterCount, builder);
        builder.Append(") ");
        builder.Append(GetOperator(specification.Operator));
        builder.Append(" (");
        WhereArgument(specification.Right, ref currParameterCount, builder);
        builder.Append(')');
    }

    private void WhereArgument(WhereArgument argument, ref int currParameterCount, StringBuilder builder)
    {
        switch (argument.Type)
        {
            case WhereArgumentType.FIELD :
                builder.Append(argument.Text);
                break;
            case WhereArgumentType.ARGUMENT :
                Where(argument.Argument, ref currParameterCount, builder);
                break;
            case WhereArgumentType.PARAMETER :
                builder.Append(GetParameter(currParameterCount));
                currParameterCount++;
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
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    protected abstract string TranslateDBFieldType(DBFieldType type);

    protected abstract string AutoIncrementAttribute { get; }
    
    protected abstract bool PrimaryAutoIncrementByDefault { get; }
}