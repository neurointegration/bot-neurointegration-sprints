using Neurointegration.Api.DataModels.Models;
using Neurointegration.Api.Storages.RoutineActions;
using Ydb.Sdk.Value;

namespace Neurointegration.Api.Storages.Mapper;

public class RoutineActionsMapper
{
    private readonly RoutineTableSchema tableSchema;

    public RoutineActionsMapper(RoutineTableSchema tableSchema)
    {
        this.tableSchema = tableSchema;
    }

    public WeekRoutineAction ToWeekRoutineAction(ResultSet.Row row, int weekNumber)
    {
        var weekCount = weekNumber switch
        {
            0 => tableSchema.WeekOneCount.GetValue(row),
            1 => tableSchema.WeekTwoCount.GetValue(row),
            2 => tableSchema.WeekThreeCount.GetValue(row),
            3 => tableSchema.WeekFourCount.GetValue(row),
        };
        return new WeekRoutineAction()
        {
            Action = tableSchema.Action.GetValue(row),
            Type = Enum.Parse<RoutineType>(tableSchema.Type.GetValue(row)),
            ActionId = tableSchema.ActionId.GetValue(row),
            WeekCount = weekCount
        };
    }
}