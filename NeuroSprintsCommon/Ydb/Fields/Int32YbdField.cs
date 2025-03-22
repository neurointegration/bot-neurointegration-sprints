using Ydb.Sdk.Value;

namespace Common.Ydb.Fields;

public class Int32YbdField : YdbFieldBase<int>
{
    public Int32YbdField(string name, params FieldConditions[] conditions) : base(name, YdbFieldTypes.Int32, conditions)
    {
    }

    protected override YdbValue Transform(int value)
    {
        if (Conditions.Contains(FieldConditions.NotNull))
            return YdbValue.MakeInt32(value);
        
        return YdbValue.MakeOptionalInt32(value);
    }

    public override int GetValue(ResultSet.Row row)
    {
        return row[Name].GetInt32();
    }
}