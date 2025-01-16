using Neurointegration.Api.DataModels.Models;

namespace Neurointegration.Api.Services;

public interface IRoutineActionsService
{
    Task<List<WeekRoutineAction>> GetWeekActions(long userId);
    Task AddAction(long userId, RoutineAction routineAction);
    Task DeleteAction(long userId, string actionId);
    Task CheckupAction(long userId, string actionId);
    Task<int> GetWeekResult(long userId, string actionId, int weekNumber);
}