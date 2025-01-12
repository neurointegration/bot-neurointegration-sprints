using Neurointegration.Api.DataModels.Models;
using Neurointegration.Api.Excpetions;
using Neurointegration.Api.Storages.RoutineActions;

namespace Neurointegration.Api.Services;

public class RoutineActionsService : IRoutineActionsService
{
    private readonly ISprintService sprintService;
    private readonly IRoutineActionsStorage routineActionsStorage;

    public RoutineActionsService(ISprintService sprintService, IRoutineActionsStorage routineActionsStorage)
    {
        this.sprintService = sprintService;
        this.routineActionsStorage = routineActionsStorage;
    }

    public async Task<List<WeekRoutineAction>> GetWeekActions(long userId)
    {
        var lastSprint = await GetLastSprint(userId);
        var weekNumber = lastSprint.GetSprintWeek();

        return await routineActionsStorage.GetActions(userId, lastSprint.SprintNumber, weekNumber);
    }

    public async Task AddAction(long userId, RoutineAction weekRoutineAction)
    {
        var lastSprint = await GetLastSprint(userId);
        await routineActionsStorage.AddAction(userId, lastSprint.SprintNumber, weekRoutineAction);
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