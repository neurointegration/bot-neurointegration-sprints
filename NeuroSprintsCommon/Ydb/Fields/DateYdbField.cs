using Ydb.Sdk.Value;

namespace Common.Ydb.Fields;

public class DateYdbField : YdbFieldBase<DateTime>
{
    public DateYdbField(string name, params FieldConditions[] conditions) : base(name, YdbFieldTypes.Date, conditions)
    {
    }

    protected override YdbValue Transform(DateTime value)
    {
        if (Conditions.Contains(FieldConditions.NotNull))
            return YdbValue.MakeDate(value);

        return YdbValue.MakeOptionalDate(value);
    }

    public override DateTime GetValue(ResultSet.Row row)
    {
        return row[Name].GetDate();
    }
}