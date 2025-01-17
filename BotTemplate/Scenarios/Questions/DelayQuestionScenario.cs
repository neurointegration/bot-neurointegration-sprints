using BotTemplate.Client;
using BotTemplate.Extensions;
using BotTemplate.Models.Telegram;
using BotTemplate.Services.Telegram;
using BotTemplate.Services.YDB;

namespace BotTemplate.Scenarios.Questions;

public class DelayQuestionScenario : IScenario
{
    private readonly IBackendApiClient backendApiClient;
    private readonly IMessageSender messageSender;
    private readonly ScenariosToStartRepository scenariosToStartRepository;
    private const string Command = CommandsConstants.DelayQuestionActionPrefix;

    public DelayQuestionScenario(
        IBackendApiClient backendApiClient,
        IMessageSender messageSender,
        ScenariosToStartRepository scenariosToStartRepository)
    {
        this.backendApiClient = backendApiClient;
        this.messageSender = messageSender;
        this.scenariosToStartRepository = scenariosToStartRepository;
    }

    public async Task<bool> TryHandle(TelegramEvent telegramEvent, CurrentScenarioInfo? scenarioInfo)
    {
        var chatId = telegramEvent.ChatId;

        var text = telegramEvent.Text ?? "";
        var splittedText = text.Split();

        if (splittedText.Length != 2) 
            return false;

        var delayQuestionCommand = splittedText[0];
        var scenarioToStartId = splittedText[1];

        if (delayQuestionCommand != Command)
            return false;

        var scenarioToStart = await scenariosToStartRepository.GetScenarioToStart(scenarioToStartId);
        if (scenarioToStart is null)
            return false;
        await scenariosToStartRepository.DeleteScenarioToStart(scenarioToStartId);

        var question = ScenarioToStartExtensions.ToQuestion(scenarioToStart);
        
        var result = await backendApiClient.CreateDelayedQuestion(question);
        
        if (!result.IsSuccess)
            await messageSender.TrySay("При откладывании сценария что-то пошло не так...", chatId);
        else
            await messageSender.TrySay("Хорошо, я напомню тебе заполнить через час", chatId);
        return true;
    }
}