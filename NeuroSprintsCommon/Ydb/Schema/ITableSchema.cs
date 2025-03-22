using System.Collections.Immutable;
using Common.Ydb.Fields;

namespace Common.Ydb.Schema;

public interface ITableSchema
{
    string TableName { get; }
    
    public IImmutableList<YdbField> Fields { get; }
    
    public string DeclareFields();
}