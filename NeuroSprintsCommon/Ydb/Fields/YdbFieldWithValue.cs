using Ydb.Sdk.Value;

namespace Common.Ydb.Fields;

public class YdbFieldWithValue: YdbField
{
    public YdbFieldWithValue(YdbField field, YdbValue value)
    {
        Type = field.Type;
        Name = field.Name;
        Conditions = field.Conditions;
        Value = value;
    }

    public override string Type { get; }
    public override string Name { get; }
    public YdbValue Value { get; }
}