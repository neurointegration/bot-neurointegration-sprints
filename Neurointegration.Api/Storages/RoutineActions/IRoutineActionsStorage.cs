using Neurointegration.Api.DataModels.Models;

namespace Neurointegration.Api.Storages.RoutineActions;

public interface IRoutineActionsStorage
{
    Task<List<WeekRoutineAction>> GetActions(long userId, long sprintNumber, int weekNumber);
    Task AddAction(long userId, long sprintNumber, RoutineAction weekRoutineAction);
    Task DeleteAction(long userId, long sprintNumber, string actionId);
    Task CheckupAction(long userId, long sprintNumber, string actionId, int weekNumber);
    Task<int> GetWeekResult(long userId, long sprintNumber, string actionId, int weekNumber);
}