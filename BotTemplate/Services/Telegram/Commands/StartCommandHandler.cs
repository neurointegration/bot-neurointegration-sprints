using BotTemplate.Services.YDB;
using BotTemplate.Services.YDB.Repo;

namespace BotTemplate.Services.Telegram.Commands;

public class StartCommandHandler : IChatCommandHandler
{
    public string Command => "/start";
    private readonly IRepo _repo;

    public StartCommandHandler(IRepo repo)
    {
        _repo = repo;
    }

    public async Task<string?> HandlePlainText(long fromChatId)
    {
        if (_repo is not CurrentScenarioRepo currentScenarioRepo)
            throw new ArgumentException("Передан неверный тип репозитория");

        return await currentScenarioRepo.StartNewScenarioAndGetMessage(fromChatId, 0);
    }
}