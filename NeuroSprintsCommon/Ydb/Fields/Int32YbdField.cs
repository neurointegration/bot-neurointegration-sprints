using Ydb.Sdk.Value;

namespace Common.Ydb.Fields;

public class Int32YbdField: YdbFieldBase<int>
{
    public Int32YbdField(string name) : base(name, YdbFieldTypes.Int32)
    {
    }

    protected override YdbValue Transform(int value)
    {
        return YdbValue.MakeInt32(value);
    }
}