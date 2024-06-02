using BotTemplate.Services.YDB;
using BotTemplate.Services.YDB.Repo;

namespace BotTemplate.Services.Telegram.Commands;

public class GetButtonsScenarioHandler : IChatCommandHandler
{
    public string Command => "/get_buttons_scenario";
    private readonly IRepo _repo;
    
    public GetButtonsScenarioHandler(IRepo repo)
    {
        _repo = repo;
    }
    
    public async Task<string?> HandlePlainText(long fromChatId)
    {
        if (_repo is not CurrentScenarioRepo currentScenarioRepo)
            throw new ArgumentException("Передан неверный тип репозитория");
        
        return string.Join('\n', await currentScenarioRepo.StartNewScenarioAndGetMessage(fromChatId, 1));
    }
}