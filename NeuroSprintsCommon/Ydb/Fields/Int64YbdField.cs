using Ydb.Sdk.Value;

namespace Common.Ydb.Fields;

public class Int64YbdField: YdbFieldBase<long>
{
    public Int64YbdField(string name, params FieldConditions[] conditions) : base(name, YdbFieldTypes.Int64, conditions)
    {
    }

    protected override YdbValue Transform(long value)
    {
        return YdbValue.MakeInt64(value);
    }

    public override long GetValue(ResultSet.Row row)
    {
        return row[Name].GetInt64();
    }
}