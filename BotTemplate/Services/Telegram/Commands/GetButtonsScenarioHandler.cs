using BotTemplate.Services.YDB;
using BotTemplate.Services.YDB.Repo;

namespace BotTemplate.Services.Telegram.Commands;

public class GetButtonsScenarioHandler : IChatCommandHandler
{
    public string Command => "/get_buttons_scenario";
    private readonly IRepository repository;
    
    public GetButtonsScenarioHandler(IRepository repository)
    {
        this.repository = repository;
    }
    
    public async Task<string?> HandlePlainText(long fromChatId)
    {
        if (repository is not ScenarioStateRepository currentScenarioRepo)
            throw new ArgumentException("Передан неверный тип репозитория");
        
        return string.Join('\n', await currentScenarioRepo.StartNewScenarioAndGetMessage(fromChatId, 1));
    }
}