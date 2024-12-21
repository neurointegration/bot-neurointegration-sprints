namespace Common.Ydb.Fields;

public interface IYdbField
{
    string Type { get; }
    string Name { get; }
    virtual string NameDeclare => $"${Name}";
    virtual string FieldDeclare => $"DECLARE {NameDeclare} AS {Type};";
    
    public string GetParameterKey() =>
        NameDeclare;
}