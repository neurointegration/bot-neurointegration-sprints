using Neurointegration.Api.DataModels.Models;

namespace Neurointegration.Api.Services.Decorators;

public class RoutineActionsServiceDecorator: IRoutineActionsService
{
    private readonly IRoutineActionsService service;
    private readonly ILogger logger;

    public RoutineActionsServiceDecorator(IRoutineActionsService service, ILogger logger)
    {
        this.service = service;
        this.logger = logger;
    }
    
    public async Task<List<WeekRoutineAction>> GetWeekActions(long userId)
    {
        logger.LogInformation("Пробуем получить рутинные действия на этой неделе");
        var result = await service.GetWeekActions(userId);
        logger.LogInformation($"Получили {result.Count} дейтсвий");

        return result;
    }

    public async Task AddAction(long userId, RoutineAction weekRoutineAction)
    {
        logger.LogInformation($"Пробуем добавить рутинные действия {weekRoutineAction}");
        await service.AddAction(userId, weekRoutineAction);
    }

    public async Task DeleteAction(long userId, string actionId)
    {
        logger.LogInformation($"Пробуем удалить рутинное действие {actionId} для {userId}");
        await service.DeleteAction(userId, actionId);
    }

    public async Task CheckupAction(long userId, string actionId)
    {
        logger.LogInformation($"Пробуем отметить выполеннным действие {actionId} для {userId}");
        await service.CheckupAction(userId, actionId);
    }

    public async Task<int> GetWeekResult(long userId, string actionId, int weekNumber)
    {
        return await service.GetWeekResult(userId, actionId, weekNumber);
    }
}