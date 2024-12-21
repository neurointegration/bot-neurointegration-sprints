using Ydb.Sdk.Value;

namespace Common.Ydb.Fields;

public class Utf8YdbField: YdbFieldBase<string>
{
    public Utf8YdbField(string name) : base(name, YdbFieldTypes.Utf8)
    {
    }

    protected override YdbValue Transform(string value)
    {
        return YdbValue.MakeUtf8(value);
    }
}