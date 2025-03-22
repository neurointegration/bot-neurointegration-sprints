using Ydb.Sdk.Value;

namespace Common.Ydb.Fields;

public abstract class YdbFieldBase<T> : YdbField
{
    public override string Type { get; }
    public override string Name { get; }

    protected abstract YdbValue Transform(T value);

    protected YdbFieldBase(string name, string type, FieldConditions[] conditions)
    {
        Name = name;
        Type = type;
        Conditions = conditions.ToHashSet();
    }

    public abstract T GetValue(ResultSet.Row row);

    public YdbFieldWithValue WithValue(T value) => new(this, Transform(value));
}