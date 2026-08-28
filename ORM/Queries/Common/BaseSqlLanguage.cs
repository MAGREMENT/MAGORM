using ORM.Abstract;
using ORM.Queries.Builder;
using ORM.Queries.Specifications;

namespace ORM.Queries.Common;

public abstract class BaseSqlLanguage : ISqlLanguage
{
    public void Create(IScriptBuilder builder, CreateSpecification specification)
    {
        builder.Append($"CREATE TABLE {specification.Model} (\n");

        var pkDone = false;
        var first = true;
        for(int i = 0; i < specification.Fields.Count; i++)
        {
            if (first) first = false;
            else builder.Append(", ");
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
            builder.Append("\n");
        }

        if (!pkDone)
        {
            if (first) first = false;
            else builder.Append(", ");
            builder.Append("    PRIMARY KEY (");
            foreach (var n in specification.PrimaryKey.Names)
            {
                builder.Append(' ');
                builder.Append(n);
            }
            builder.Append(")");
        }
        
        foreach (var fk in specification.ForeignKeys)
        {
            if (first) first = false;
            else builder.Append(", ");
            builder.Append($"    FOREIGN KEY ({fk.Field}) REFERENCES {fk.OtherModel}({fk.OtherField})");
        }
        
        builder.Append(')');
        builder.EndStatement();
    }

    public void CreateFromSelect(IScriptBuilder builder, CreateFromSelectSpecification specification)
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
        SelectInternal(builder, specification.Select);
        builder.EndStatement();
    }

    public void Insert(IScriptBuilder builder, InsertSpecification specification)
    {
        builder.Append($"INSERT INTO {specification.Model} (");
        for (int i = 0; i < specification.Fields.Count; i++)
        {
            if (i > 0) builder.Append(", ");
            builder.Append(specification.Fields[i]);
        }

        builder.Append(")\nVALUES (");
        for (int i = 0; i < specification.Values.Count; i++)
        {
            if (i > 0) builder.Append(", ");
            builder.AppendParameter(specification.Values[i]);
        }

        builder.Append(')');
        AddInsertFieldReturns(builder, specification);
        builder.EndStatement();
    }

    protected virtual void AddInsertFieldReturns(IScriptBuilder builder,
        InsertSpecification specification) {}

    public void Update(IScriptBuilder builder, UpdateSpecification specification)
    {
        builder.Append($"UPDATE {specification.Model}\nSET");

        for (int i = 0; i < specification.Fields.Count; i++)
        {
            builder.Append(' ');
            if (i > 0) builder.Append(", ");
            builder.Append(specification.Fields[i]);
            builder.Append(" = ");
            builder.AppendParameter(specification.Values[i]);
        }

        if (specification.Where is not null) Where(builder, specification.Where);
        builder.EndStatement();
    }

    public void Select(IScriptBuilder builder, SelectSpecification specification)
    {
        SelectInternal(builder, specification);
        builder.EndStatement();
    }

    public void SelectInternal(IScriptBuilder builder, SelectSpecification specification)
    {
        builder.Append("SELECT ");

        if (specification.Fields is null) builder.Append('*');
        else
        {
            var isFirst = true;
            foreach (var field in specification.Fields)
            {
                if (isFirst) isFirst = false;
                else builder.Append(", ");

                if (field.IsTableColumn) builder.Append(field.ToString()!);
                else builder.AppendParameter(field.Value);

                if (field.Alias is not null)
                {
                    builder.Append(" AS ");
                    builder.Append(field.Alias);
                }
            }
        }

        builder.Append($"\nFROM {specification.Model}");

        if (specification.Where is not null) Where(builder, specification.Where);
        if (specification.OrderBy is not null && specification.OrderBy.Length > 0)
        {
            builder.Append("\nORDER BY");
            for (int i = 0; i < specification.OrderBy.Length; i++)
            {
                builder.Append(' ');
                if (i > 0) builder.Append(", ");
                builder.Append(specification.OrderBy[i].Field);
                builder.Append(' ');
                builder.Append(specification.OrderBy[i].Type.ToString());
            }
        }
    }

    public void Delete(IScriptBuilder builder, DeleteSpecification specification)
    {
        builder.Append("DELETE FROM ");
        builder.Append(specification.Model);
        if (specification.Where is not null) Where(builder, specification.Where);
        builder.EndStatement();
    }

    public void Drop(IScriptBuilder builder, string name)
    {
        builder.Append("DROP TABLE ");
        builder.Append(name);
        builder.EndStatement();
    }

    public virtual bool Nuke(IScriptBuilder builder)
    {
        return false;
    }

    public virtual bool IsSameDBFieldType(DBFieldType left, DBFieldType right)
    {
        return left == right;
    }

    public abstract IScriptBuilder InitScriptBuilder();

    private void Where(IScriptBuilder builder, WhereSpecification specification)
    {
        builder.Append("\nWHERE");
        AddWhere(builder, specification);
    }

    //TODO limit parenthesis more
    private void AddWhere(IScriptBuilder builder, WhereSpecification specification)
    {
        var i = 0;
        builder.Append(' ');
        if(specification.Left.Type == WhereArgumentType.ARGUMENT) builder.Append('(');
        WhereArgument(builder, specification.Left, specification.Values, ref i);
        if(specification.Left.Type == WhereArgumentType.ARGUMENT) builder.Append(')');
        builder.Append(' ');
        builder.Append(GetOperator(specification.Operator));
        builder.Append(' ');
        if(specification.Right.Type == WhereArgumentType.ARGUMENT) builder.Append('(');
        WhereArgument(builder, specification.Right, specification.Values, ref i);
        if(specification.Right.Type == WhereArgumentType.ARGUMENT) builder.Append(')');
    }

    private void WhereArgument(IScriptBuilder builder, WhereArgument argument, IReadOnlyList<object?> parameters, ref int i)
    {
        switch (argument.Type)
        {
            case WhereArgumentType.FIELD :
                builder.Append(argument.Text);
                break;
            case WhereArgumentType.ARGUMENT :
                AddWhere(builder, argument.Argument);
                break;
            case WhereArgumentType.PARAMETER :
                builder.AppendParameter(parameters[i++]);
                break;
        }
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