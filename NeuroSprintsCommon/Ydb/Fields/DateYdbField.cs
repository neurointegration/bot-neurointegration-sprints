using Ydb.Sdk.Value;

namespace Common.Ydb.Fields;

public class DateYdbField: YdbFieldBase<DateTime>
{
    public DateYdbField(string name) : base(name, YdbFieldTypes.Date)
    {
    }

    protected override YdbValue Transform(DateTime value)
    {
        return YdbValue.MakeDate(value);
    }
}