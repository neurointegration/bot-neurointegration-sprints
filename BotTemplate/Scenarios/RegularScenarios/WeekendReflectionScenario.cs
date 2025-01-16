using BotTemplate.Client;
using BotTemplate.Models.ScenariosData;
using BotTemplate.Models.Telegram;
using BotTemplate.Services.Telegram;
using BotTemplate.Services.YDB;
using Neurointegration.Api.DataModels.Dto;
using Neurointegration.Api.DataModels.Models;
using Newtonsoft.Json;

namespace BotTemplate.Scenarios.RegularScenarios;

public class WeekendReflectionScenario : IRegularScenario
{
    private const string RegularReflectionScenarioId = "regular_weekend_reflection";
    private const string LastReflectionScenarioId = "last_regular_weekend_reflection";
    private const ScenarioType SelfScenarioType = ScenarioType.Reflection;
    private const int SprintReplyCount = 4;

    private readonly ScenarioStateRepository scenarioStateRepository;
    private readonly ScenariosToStartRepository scenariosToStartRepository;
    private readonly IMessageSender messageSender;
    private readonly IBackendApiClient backendApiClient;

    public WeekendReflectionScenario(
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

        string scenarioToStartId;
        if (question.SprintReplyNumber == SprintReplyCount - 1)
        {
            scenarioToStartId = await scenariosToStartRepository.AddNewScenarioToStartAndGetItsId(question.UserId, LastReflectionScenarioId,
            question.Date, question.SprintNumber, question.SprintReplyNumber);
        }
        else
        {
            scenarioToStartId = await scenariosToStartRepository.AddNewScenarioToStartAndGetItsId(question.UserId, RegularReflectionScenarioId,
            question.Date, question.SprintNumber, question.SprintReplyNumber);
        }

        await messageSender.TrySay(WeekendReflectionMessages.AskReady(scenarioToStartId), question.UserId);

        return true;
    }

    public async Task<bool> TryStart(Question question)
    {
        if (question.ScenarioType != SelfScenarioType)
            return false;

        if (question.SprintReplyNumber == SprintReplyCount - 1)
        {
            await scenarioStateRepository.StartNewScenario(question.UserId,
                LastReflectionScenarioId, question.Date, question.SprintNumber, question.SprintReplyNumber);
            await scenarioStateRepository.UpdateData(question.UserId,
                new RegularScenarioData() {AnswerType = AnswerType.ReflectionIntegrationChanges});
            await messageSender.TrySay(WeekendReflectionMessages.AskChanges(), question.UserId);
        }
        else
        {
            await scenarioStateRepository.StartNewScenario(question.UserId,
                RegularReflectionScenarioId, question.Date, question.SprintNumber, question.SprintReplyNumber);
            await scenarioStateRepository.UpdateData(question.UserId,
                new RegularScenarioData() {AnswerType = AnswerType.ReflectionRegularWhatIDoing});
            await messageSender.TrySay(WeekendReflectionMessages.AskWhatIDoing(), question.UserId);
        }

        return true;
    }

    public async Task<bool> TryHandle(TelegramEvent telegramEvent, CurrentScenarioInfo? scenarioInfo)
    {
        var chatId = telegramEvent.ChatId;
        if (scenarioInfo == null || scenarioInfo.ScenarioId != RegularReflectionScenarioId)
            return false;

        var text = telegramEvent.Text ?? "";
        var scenarioData = JsonConvert.DeserializeObject<RegularScenarioData>(scenarioInfo.Data);
        if (scenarioData == null)
            throw new ArgumentException("Не правильно указана специальная информация для сценария вечерней рефлексии");

        var sendAnswer = new SendAnswer
        {
            UserId = chatId,
            Answer = text,
            AnswerType = scenarioData.AnswerType,
            ScenarioType = SelfScenarioType,
            Date = DateOnly.FromDateTime(scenarioInfo.Date ?? DateTime.UtcNow),
            SprintNumber = (int) scenarioInfo.CurrentSprintNumber!.Value,
            SprintReplyNumber = scenarioInfo.SprintReplyNumber!.Value
        };
        await backendApiClient.SaveAnswer(sendAnswer);
        
        await AskRegularReflectionMessages(chatId, scenarioData.AnswerType);
        await AskIntegrationReflectionMessages(chatId, scenarioData.AnswerType);

        return true;
    }

    private async Task AskRegularReflectionMessages(long chatId, AnswerType answerType)
    {
        if (answerType == AnswerType.ReflectionRegularWhatIDoing)
        {
            await messageSender.TrySay(WeekendReflectionMessages.AskWhatINotDoing(), chatId);
            await scenarioStateRepository.UpdateData(chatId,
                new RegularScenarioData() {AnswerType = AnswerType.ReflectionRegularWhatINotDoing});
        }

        if (answerType == AnswerType.ReflectionRegularWhatINotDoing)
        {
            await messageSender.TrySay(WeekendReflectionMessages.AskStatus(), chatId);
            await scenarioStateRepository.UpdateData(chatId,
                new RegularScenarioData() {AnswerType = AnswerType.ReflectionRegularMyStatus});
        }

        if (answerType == AnswerType.ReflectionRegularMyStatus)
        {
            await messageSender.TrySay(WeekendReflectionMessages.AskOrbits(), chatId);
            await scenarioStateRepository.UpdateData(chatId,
                new RegularScenarioData() {AnswerType = AnswerType.ReflectionRegularOrbits});
        }

        if (answerType == AnswerType.ReflectionRegularOrbits)
        {
            await messageSender.TrySay(WeekendReflectionMessages.AskCorrection(), chatId);
            await scenarioStateRepository.UpdateData(chatId,
                new RegularScenarioData() {AnswerType = AnswerType.ReflectionRegularCorrection});
        }

        if (answerType == AnswerType.ReflectionRegularCorrection)
        {
            await messageSender.TrySay(WeekendReflectionMessages.FinishReflection(), chatId);
            await scenarioStateRepository.EndScenarioNoMatterWhat(chatId);
        }
    }

    private async Task AskIntegrationReflectionMessages(long chatId, AnswerType answerType)
    {
        if (answerType == AnswerType.ReflectionIntegrationChanges)
        {
            await messageSender.TrySay(WeekendReflectionMessages.AskActions(), chatId);
            await scenarioStateRepository.UpdateData(chatId,
                new RegularScenarioData() {AnswerType = AnswerType.ReflectionIntegrationActions});
        }

        if (answerType == AnswerType.ReflectionIntegrationActions)
        {
            await messageSender.TrySay(WeekendReflectionMessages.AskAbilities(), chatId);
            await scenarioStateRepository.UpdateData(chatId,
                new RegularScenarioData() {AnswerType = AnswerType.ReflectionIntegrationAbilities});
        }

        if (answerType == AnswerType.ReflectionIntegrationAbilities)
        {
            await messageSender.TrySay(WeekendReflectionMessages.AskBeliefs(), chatId);
            await scenarioStateRepository.UpdateData(chatId,
                new RegularScenarioData() {AnswerType = AnswerType.ReflectionIntegrationBeliefs});
        }

        if (answerType == AnswerType.ReflectionIntegrationBeliefs)
        {
            await messageSender.TrySay(WeekendReflectionMessages.AskSelfPerception(), chatId);
            await scenarioStateRepository.UpdateData(chatId,
                new RegularScenarioData() {AnswerType = AnswerType.ReflectionIntegrationSelfPerception});
        }

        if (answerType == AnswerType.ReflectionIntegrationSelfPerception)
        {
            await messageSender.TrySay(WeekendReflectionMessages.AskOpportunities(), chatId);
            await scenarioStateRepository.UpdateData(chatId,
                new RegularScenarioData() {AnswerType = AnswerType.ReflectionIntegrationOpportunities});
        }

        if (answerType == AnswerType.ReflectionIntegrationOpportunities)
        {
            await messageSender.TrySay(WeekendReflectionMessages.FinishReflection(), chatId);
            await scenarioStateRepository.EndScenarioNoMatterWhat(chatId);
        }
    }
}