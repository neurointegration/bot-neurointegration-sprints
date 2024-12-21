using Ydb.Sdk.Value;

namespace Common.Ydb.Fields;

public abstract class YdbFieldBase<T> : IYdbField
{
    public string Type { get; }
    public string Name { get; }

    protected abstract YdbValue Transform(T value);

    protected YdbFieldBase(string name, string type)
    {
        Name = name;
        Type = type;
    }

    public YdbValue GetParameterValue(T value) =>
        Transform(value);

    public string NameDeclare => $"${Name}";
    public string FieldDeclare => $"DECLARE {NameDeclare} AS {Type};";

    public string GetParameterKey() =>
        NameDeclare;
}