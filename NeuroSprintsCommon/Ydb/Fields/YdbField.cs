namespace Common.Ydb.Fields;

public abstract class YdbField
{
    public abstract string Type { get; }
    public abstract string Name { get; }
    public HashSet<FieldConditions> Conditions { get; protected init; }
    public string NameDeclare => $"${Name}";
    public string FieldDeclare => $"DECLARE {NameDeclare} AS {Type};";
}