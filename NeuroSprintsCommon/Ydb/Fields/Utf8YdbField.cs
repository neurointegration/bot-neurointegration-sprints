using Ydb.Sdk.Value;

namespace Common.Ydb.Fields;

public class Utf8YdbField: YdbFieldBase<string>
{
    public Utf8YdbField(string name, params FieldConditions[] conditions) : base(name, YdbFieldTypes.Utf8, conditions)
    {
    }

    protected override YdbValue Transform(string value)
    {
        if (Conditions.Contains(FieldConditions.NotNull))
            return YdbValue.MakeUtf8(value);
        
        return YdbValue.MakeOptionalUtf8(value);
    }

    public override string GetValue(ResultSet.Row row)
    {
        if (Conditions.Contains(FieldConditions.NotNull))
            return row[Name].GetUtf8();
        
        return row[Name].GetOptionalUtf8();
    }
}