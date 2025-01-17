using BotTemplate.Client;
using BotTemplate.Models.Telegram;
using BotTemplate.Services.Telegram;
using BotTemplate.Services.YDB;
using Neurointegration.Api.DataModels.Dto;
using Neurointegration.Api.DataModels.Models;

namespace BotTemplate.Scenarios.RegularScenarios;

public class StatusScenario : IRegularScenario
{
    private const string ScenarioId = "regular_status";
    private const ScenarioType SelfScenarioType = ScenarioType.Status;

    private readonly ScenariosToStartRepository scenariosToStartRepository;
    private readonly IMessageSender messageSender;
    private readonly IBackendApiClient backendApiClient;

    private readonly HashSet<string> commands = new HashSet<string>()
    {
        CommandsConstants.StatusPanicActionPrefix,
        CommandsConstants.StatusOverexcitationActionPrefix,
        CommandsConstants.StatusInclusionActionPrefix,
        CommandsConstants.StatusBalanceActionPrefix,
        CommandsConstants.StatusRelaxationActionPrefix,
        CommandsConstants.StatusPassivityActionPrefix,
        CommandsConstants.StatusApathyActionPrefix
    };

    public StatusScenario(
        ScenariosToStartRepository scenariosToStartRepository,
        IMessageSender messageSender,
        IBackendApiClient backendApiClient)
    {
        this.scenariosToStartRepository = scenariosToStartRepository;
        this.messageSender = messageSender;
        this.backendApiClient = backendApiClient;
    }

    public async Task<bool> TryAddToStart(Question question)
    {
        if (question.ScenarioType != SelfScenarioType)
            return false;

        var scenarioToStartId = await scenariosToStartRepository.AddNewScenarioToStartAndGetItsId(question.UserId, ScenarioId, question.Priority, question.ScenarioType,
            question.Date, question.SprintNumber, question.SprintReplyNumber, question.IsDelayed);

        await messageSender.TrySay(StateMessages.AskStatus(scenarioToStartId), question.UserId);

        return true;
    }

    public async Task<bool> TryHandle(TelegramEvent telegramEvent, CurrentScenarioInfo? scenarioInfo)
    {
        var chatId = telegramEvent.ChatId;

        var text = telegramEvent.Text ?? "";
        var splittedText = text.Split();

        if (splittedText.Length != 2) 
            return false;

        var userStatusCommand = splittedText[0];
        var scenarioToStartId = splittedText[1];

        if (!commands.Contains(userStatusCommand))
            return false;

        var scenarioToStart = await scenariosToStartRepository.GetScenarioToStart(scenarioToStartId);
        if (scenarioToStart is null || scenarioToStart.ScenarioType is not SelfScenarioType)
            return false;
        await scenariosToStartRepository.DeleteScenarioToStart(scenarioToStartId);

        var userStatus = userStatusCommand switch
        {
            CommandsConstants.StatusPanicActionPrefix => CommandsConstants.StatusPanic,
            CommandsConstants.StatusOverexcitationActionPrefix => CommandsConstants.StatusOverexcitation,
            CommandsConstants.StatusInclusionActionPrefix => CommandsConstants.StatusInclusion,
            CommandsConstants.StatusBalanceActionPrefix => CommandsConstants.StatusBalance,
            CommandsConstants.StatusRelaxationActionPrefix => CommandsConstants.StatusRelaxation,
            CommandsConstants.StatusPassivityActionPrefix => CommandsConstants.StatusPassivity,
            CommandsConstants.StatusApathyActionPrefix => CommandsConstants.StatusApathy,
            _ => throw new ArgumentException("Не существует такой команды статуса")
        };

        var sendAnswer = new SendAnswer
        {
            UserId = chatId,
            Answer = userStatus,
            AnswerType = AnswerType.Status,
            ScenarioType = SelfScenarioType,
            Date = DateOnly.FromDateTime(scenarioToStart.Date ?? DateTime.UtcNow),
            SprintNumber = (int) scenarioToStart.SprintNumber,
            SprintReplyNumber = scenarioToStart.SprintReplyNumber
        };
        await backendApiClient.SaveAnswer(sendAnswer);

        await messageSender.TrySay(StateMessages.GetRecommendation(userStatus), chatId);
        return true;
    }
}