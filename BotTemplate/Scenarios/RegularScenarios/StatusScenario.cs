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

    private readonly ScenarioStateRepository scenarioStateRepository;
    private readonly ScenariosToStartRepository scenariosToStartRepository;
    private readonly IMessageSender messageSender;
    private readonly IBackendApiClient backendApiClient;

    private readonly HashSet<string> commands = new HashSet<string>()
    {
        CommandsConstants.StatusPanic,
        CommandsConstants.StatusOverexcitation,
        CommandsConstants.StatusInclusion,
        CommandsConstants.StatusBalance,
        CommandsConstants.StatusRelaxation,
        CommandsConstants.StatusPassivity,
        CommandsConstants.StatusApathy,
    };

    public StatusScenario(
        ScenarioStateRepository scenarioStateRepository,
        ScenariosToStartRepository scenariosToStartRepository,
        IMessageSender messageSender,
        IBackendApiClient backendApiClient)
    {
        this.scenarioStateRepository = scenarioStateRepository;
        this.scenariosToStartRepository = scenariosToStartRepository;
        this.messageSender = messageSender;
        this.backendApiClient = backendApiClient;
    }

    public async Task<bool> TryAddToStart(Question question)
    {
        if (question.ScenarioType != SelfScenarioType)
            return false;

        var scenarioToStartId = await scenariosToStartRepository.AddNewScenarioToStartAndGetItsId(question.UserId, ScenarioId,
            question.Date, question.SprintNumber, question.SprintReplyNumber);

        await messageSender.TrySay(StateMessages.AskStatus(scenarioToStartId), question.UserId);

        return true;
    }

    public async Task<bool> TryStart(Question question)
    {
        if (question.ScenarioType != SelfScenarioType)
            return false;

        await scenarioStateRepository.StartNewScenario(question.UserId, ScenarioId,
            question.Date, question.SprintNumber, question.SprintReplyNumber);

        return true;
    }

    public async Task<bool> TryHandle(TelegramEvent telegramEvent, CurrentScenarioInfo? scenarioInfo)
    {
        var chatId = telegramEvent.ChatId;
        if (scenarioInfo == null || scenarioInfo.ScenarioId != ScenarioId)
            return false;

        var text = telegramEvent.Text ?? "";

        if (!commands.Contains(text))
        {
            await messageSender.TrySay(StateMessages.GetRecommendation(text), chatId);
            return true;
        }

        var sendAnswer = new SendAnswer
        {
            UserId = chatId,
            Answer = text,
            AnswerType = AnswerType.Status,
            ScenarioType = SelfScenarioType,
            Date = DateOnly.FromDateTime(scenarioInfo.Date ?? DateTime.UtcNow),
            SprintNumber = (int) scenarioInfo.CurrentSprintNumber!.Value,
            SprintReplyNumber = scenarioInfo.SprintReplyNumber!.Value
        };
        await backendApiClient.SaveAnswer(sendAnswer);

        await messageSender.TrySay(StateMessages.GetRecommendation(text), chatId);
        await scenarioStateRepository.EndScenarioNoMatterWhat(chatId);
        return true;
    }
}