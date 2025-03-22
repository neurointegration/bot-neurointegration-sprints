using System.Collections.Immutable;
using Common.Ydb.Fields;
using Common.Ydb.Schema;

namespace Neurointegration.Api.Storages.RoutineActions;

public class RoutineTableSchema : BaseTableSchema
{
    public override string TableName => "routine_actions_info";
    public override IImmutableList<YdbField> Fields { get; }

    public Int64YbdField UserId { get; set; }
    public Int64YbdField SprintNumber { get; set; }
    public Utf8YdbField ActionId { get; set; }
    public Utf8YdbField Type { get; set; }
    public Utf8YdbField Action { get; set; }
    public Int32YbdField WeekOneCount { get; set; }
    public Int32YbdField WeekTwoCount { get; set; }
    public Int32YbdField WeekThreeCount { get; set; }
    public Int32YbdField WeekFourCount { get; set; }

    public RoutineTableSchema()
    {
        UserId = new Int64YbdField("user_id", FieldConditions.NotNull, FieldConditions.IsPrimaryKey);
        SprintNumber = new Int64YbdField("sprint_number", FieldConditions.NotNull, FieldConditions.IsPrimaryKey);
        ActionId = new Utf8YdbField("action_id", FieldConditions.NotNull, FieldConditions.IsPrimaryKey);
        Type = new Utf8YdbField("type", FieldConditions.NotNull);
        Action = new Utf8YdbField("action", FieldConditions.NotNull);
        WeekOneCount = new Int32YbdField("week_one", FieldConditions.NotNull);
        WeekTwoCount = new Int32YbdField("week_two", FieldConditions.NotNull);
        WeekThreeCount = new Int32YbdField("week_three", FieldConditions.NotNull);
        WeekFourCount = new Int32YbdField("week_four", FieldConditions.NotNull);

        Fields = new List<YdbField>()
        {
            UserId,
            SprintNumber,
            ActionId,
            Type,
            Action,
            WeekOneCount,
            WeekTwoCount,
            WeekThreeCount,
            WeekFourCount
        }.ToImmutableArray();
    }
}