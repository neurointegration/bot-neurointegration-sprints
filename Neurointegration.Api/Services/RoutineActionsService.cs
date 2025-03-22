using Neurointegration.Api.DataModels.Models;
using Neurointegration.Api.Excpetions;
using Neurointegration.Api.Google;
using Neurointegration.Api.Storages;
using Neurointegration.Api.Storages.RoutineActions;

namespace Neurointegration.Api.Services;

public class RoutineActionsService : IRoutineActionsService
{
    private readonly ISprintService sprintService;
    private readonly IRoutineActionsStorage routineActionsStorage;
    private readonly IGoogleStorage googleStorage;
    private readonly GoogleSheetUtils googleSheetUtils;

    public RoutineActionsService(
        ISprintService sprintService,
        IRoutineActionsStorage routineActionsStorage,
        IGoogleStorage googleStorage,
        GoogleSheetUtils googleSheetUtils)
    {
        this.sprintService = sprintService;
        this.routineActionsStorage = routineActionsStorage;
        this.googleStorage = googleStorage;
        this.googleSheetUtils = googleSheetUtils;
    }

    public async Task<List<WeekRoutineAction>> GetWeekActions(long userId)
    {
        var lastSprint = await GetLastSprint(userId);
        var weekNumber = lastSprint.GetSprintWeek();

        return await routineActionsStorage.GetActions(userId, lastSprint.SprintNumber, weekNumber);
    }

    public async Task AddAction(long userId, RoutineAction routineAction)
    {
        var lastSprint = await GetLastSprint(userId);
        var typeSprintNumber = routineAction.Type switch
        {
            RoutineType.Life => lastSprint.LifeCount,
            RoutineType.Pleasure => lastSprint.PleasureCount,
            RoutineType.Drive => lastSprint.DriveCount,
            _ => throw new ArgumentOutOfRangeException()
        };
        await routineActionsStorage.AddAction(userId, lastSprint.SprintNumber, typeSprintNumber, routineAction);
        var cell = googleSheetUtils.GetRoutineActionNameCell(routineAction.Type, lastSprint);
        switch (routineAction.Type)
        {
            case RoutineType.Life:
                lastSprint.LifeCount++;
                break;
            case RoutineType.Pleasure:
                lastSprint.PleasureCount++;
                break;
            case RoutineType.Drive:
                lastSprint.DriveCount++;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        await sprintService.SaveOrUpdate(lastSprint);
        googleStorage.Save(routineAction.Action, lastSprint.SheetId, cell);
    }

    public async Task DeleteAction(long userId, string actionId)
    {
        var lastSprint = await GetLastSprint(userId);
        await routineActionsStorage.DeleteAction(userId, lastSprint.SprintNumber, actionId);
    }

    public async Task CheckupAction(long userId, string actionId)
    {
        var lastSprint = await GetLastSprint(userId);
        var weekNumber = lastSprint.GetSprintWeek();
        await routineActionsStorage.CheckupAction(userId, lastSprint.SprintNumber, actionId, weekNumber);
        CheckupActionGoogleTable(userId, actionId, lastSprint, weekNumber);
    }

    private async Task CheckupActionGoogleTable(long userId, string actionId, Sprint sprint, int weekNumber)
    {
        var routineAction =
            await routineActionsStorage.GetAction(userId, sprint.SprintNumber, actionId, weekNumber);
        var cell = googleSheetUtils.GetRoutineActionCheckUpCell(routineAction.Type, int.Parse(actionId), weekNumber);
        await googleStorage.Save(routineAction.WeekCount.ToString(), sprint.SheetId, cell);
    }

    public async Task<int> GetWeekResult(long userId, string actionId, int weekNumber)
    {
        var lastSprint = await GetLastSprint(userId);
        return await routineActionsStorage.GetWeekResult(userId, lastSprint.SprintNumber, actionId, weekNumber);
    }

    private async Task<Sprint> GetLastSprint(long userId)
    {
        var lastSprint = await sprintService.GetLastSprint(userId);
        if (lastSprint == null)
            throw new NotFoundException("Не найден спринт пользователя");

        return lastSprint;
    }
}