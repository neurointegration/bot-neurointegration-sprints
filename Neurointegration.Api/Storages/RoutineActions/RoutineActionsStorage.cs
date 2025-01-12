using Common.Ydb;
using Common.Ydb.Fields;
using Neurointegration.Api.DataModels.Models;
using Neurointegration.Api.Storages.Mapper;

namespace Neurointegration.Api.Storages.RoutineActions;

public class RoutineActionsStorage : IRoutineActionsStorage
{
    private readonly YdbClient ydbClient;
    private readonly RoutineActionsMapper routineActionsMapper;
    private readonly RoutineTableSchema tableSchema;

    public RoutineActionsStorage(
        YdbClient ydbClient,
        RoutineActionsMapper routineActionsMapper,
        RoutineTableSchema tableSchema)
    {
        this.ydbClient = ydbClient;
        this.routineActionsMapper = routineActionsMapper;
        this.tableSchema = tableSchema;
    }

    public async Task<List<WeekRoutineAction>> GetActions(long userId, long sprintNumber, int weekNumber)
    {
        var result = await ydbClient.Find(tableSchema,
            tableSchema.Fields.ToArray(),
            new[]
            {
                tableSchema.UserId.WithValue(userId),
                tableSchema.SprintNumber.WithValue(sprintNumber)
            });
        
        return result.Select(row => routineActionsMapper.ToRoutineAction(row, weekNumber)).ToList();
    }

    public async Task AddAction(long userId, long sprintNumber, RoutineAction weekRoutineAction)
    {
        await ydbClient.Replace(
            tableSchema,
            new[]
            {
                tableSchema.UserId.WithValue(userId),
                tableSchema.SprintNumber.WithValue(sprintNumber),
                tableSchema.ActionId.WithValue(Guid.NewGuid().ToString()),
                tableSchema.Action.WithValue(weekRoutineAction.Action),
                tableSchema.Type.WithValue(weekRoutineAction.Type.ToString()),
                tableSchema.WeekOneCount.WithValue(0),
                tableSchema.WeekTwoCount.WithValue(0),
                tableSchema.WeekThreeCount.WithValue(0),
                tableSchema.WeekFourCount.WithValue(0)
            });
    }

    public async Task DeleteAction(long userId, long sprintNumber, string actionId)
    {
        await ydbClient.Delete(tableSchema,
            new[]
            {
                tableSchema.UserId.WithValue(userId),
                tableSchema.SprintNumber.WithValue(sprintNumber),
                tableSchema.ActionId.WithValue(actionId),
            });
    }

    public async Task CheckupAction(long userId, long sprintNumber, string actionId, int weekNumber)
    {
        var result = await GetWeekResult(userId, sprintNumber, actionId, weekNumber);
        var fieldUpdate = weekNumber switch
        {
            0 => tableSchema.WeekOneCount.WithValue(result + 1),
            1 => tableSchema.WeekTwoCount.WithValue(result + 1),
            2 => tableSchema.WeekThreeCount.WithValue(result + 1),
            3 => tableSchema.WeekFourCount.WithValue(result + 1),
        };
        
        await ydbClient.Update(tableSchema,
            new[]
            {
                tableSchema.UserId.WithValue(userId),
                tableSchema.SprintNumber.WithValue(sprintNumber),
                tableSchema.ActionId.WithValue(actionId),
            },
            new []
            {
                fieldUpdate
            });
    }

    public async Task<int> GetWeekResult(long userId, long sprintNumber, string actionId, int weekNumber)
    {
        var findField = weekNumber switch
        {
            0 => tableSchema.WeekOneCount,
            1 => tableSchema.WeekTwoCount,
            2 => tableSchema.WeekThreeCount,
            3 => tableSchema.WeekFourCount,
        };
        
        var result = await ydbClient.Find(tableSchema,
            new YdbField[]
            {
                findField
            },
            new[]
            {
                tableSchema.UserId.WithValue(userId),
                tableSchema.SprintNumber.WithValue(sprintNumber),
                tableSchema.ActionId.WithValue(actionId),
            });

        result = result.ToArray();
        
        if (!result.Any())
            throw new ArgumentException("Ошибка при получении результата недели");

        
        return findField.GetValue(result.First());
    }
}