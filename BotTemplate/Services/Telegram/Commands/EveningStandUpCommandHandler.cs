using BotTemplate.Services.YDB;
using BotTemplate.Services.YDB.Repo;

namespace BotTemplate.Services.Telegram.Commands;

public class EveningStandUpCommandHandler : IChatCommandHandler
{
    public string Command => "/evening_stand_up";
    private readonly IRepo _repo;

    public EveningStandUpCommandHandler(IRepo repo)
    {
        _repo = repo;
    }

    public async Task<string?> HandlePlainText(long fromChatId)
    {
        if (_repo is not CurrentScenarioRepository currentScenarioRepo)
            throw new ArgumentException("Передан неверный тип репозитория");

        return await currentScenarioRepo.StartNewScenarioAndGetMessage(fromChatId, 2);
    }
}