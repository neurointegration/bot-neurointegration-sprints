using BotTemplate.Services.YDB;
using BotTemplate.Services.YDB.Repo;

namespace BotTemplate.Services.Telegram.Commands;

public class WeeklyReflectionCommandHandler : IChatCommandHandler
{
    public string Command => "/weekly_reflection";
    private readonly IRepository repository;

    public WeeklyReflectionCommandHandler(IRepository repository)
    {
        this.repository = repository;
    }

    public async Task<string?> HandlePlainText(long fromChatId)
    {
        if (repository is not ScenarioStateRepository currentScenarioRepo)
            throw new ArgumentException("Передан неверный тип репозитория");

        return await currentScenarioRepo.StartNewScenarioAndGetMessage(fromChatId, 3);
    }
}