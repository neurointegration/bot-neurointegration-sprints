using Ydb.Sdk.Value;

namespace Common.Ydb.Fields;

public class Int64YbdField: YdbFieldBase<long>
{
    public Int64YbdField(string name) : base(name, YdbFieldTypes.Int64)
    {
    }

    protected override YdbValue Transform(long value)
    {
        return YdbValue.MakeInt64(value);
    }
}