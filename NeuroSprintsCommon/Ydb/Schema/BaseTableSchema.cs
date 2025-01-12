using System.Collections.Immutable;
using Common.Ydb.Fields;

namespace Common.Ydb.Schema;

public abstract class BaseTableSchema : ITableSchema
{
    public abstract string TableName { get; }
    public abstract IImmutableList<YdbField> Fields { get; }

    public string DeclareFields()
    {
        return string.Join("\n", Fields.Select(x => x.FieldDeclare));
    }
}