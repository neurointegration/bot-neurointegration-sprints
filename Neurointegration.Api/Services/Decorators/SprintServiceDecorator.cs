using Neurointegration.Api.DataModels.Models;
using Neurointegration.Api.DataModels.Result;

namespace Neurointegration.Api.Services.Decorators;

public class SprintServiceDecorator : ISprintService
{
    private readonly ISprintService sprintService;
    private readonly ILogger logger;

    public SprintServiceDecorator(ISprintService sprintService, ILogger logger)
    {
        this.sprintService = sprintService;
        this.logger = logger;
    }

    public async Task<Sprint> CreateSprint(User user, long sprintNumber, DateTime sprintStartDate)
    {
        logger.LogInformation("Создаем спринт");
        return await sprintService.CreateSprint(user, sprintNumber, sprintStartDate);
    }

    public async Task SaveOrUpdate(Sprint sprint)
    {
        logger.LogInformation($"Создаем или обновляем спринт: {sprint}");

        await sprintService.SaveOrUpdate(sprint);
    }

    public async Task<Sprint?> GetLastSprint(long userId)
    {
        logger.LogInformation($"Получаем последний спринт для {userId}");
        var result = await sprintService.GetLastSprint(userId);
        logger.LogInformation($"Получили {result}");

        return result;
    }

    public async Task<List<string>> GetUserGoogleSheets(long ownerId)
    {
        logger.LogInformation($"Получаем гугл таблицы для {ownerId}");

        return await sprintService.GetUserGoogleSheets(ownerId);
    }

    public async Task<Result<Sprint>> GetSprint(long userId, long sprintNumber)
    {
        logger.LogInformation($"Получаем спринт {sprintNumber} для {userId}");

        return await sprintService.GetSprint(userId, sprintNumber);
    }

    public async Task<List<Sprint>> GetSprints(long userId)
    {
        logger.LogInformation($"Получаем спринты для {userId}");

        return await sprintService.GetSprints(userId);
    }
}