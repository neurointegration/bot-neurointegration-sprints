using BotTemplate.Client;
using BotTemplate.Models.ScenariosData;
using BotTemplate.Models.Telegram;
using BotTemplate.Services.Telegram;
using BotTemplate.Services.YDB;
using Neurointegration.Api.DataModels.Dto;
using Neurointegration.Api.DataModels.Models;
using Newtonsoft.Json;

namespace BotTemplate.Scenarios.RegularScenarios;

public class EveningStandUpScenario : IRegularScenario
{
    private const string ScenarioId = "regular_evening_standup";
    private const ScenarioType SelfScenarioType = ScenarioType.EveningStandUp;

    private readonly ScenarioStateRepository scenarioStateRepository;
    private readonly ScenariosToStartRepository scenariosToStartRepository;
    private readonly IMessageSender messageSender;
    private readonly IBackendApiClient backendApiClient;

    public EveningStandUpScenario(
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

        var scenarioToStartId = await scenariosToStartRepository.AddNewScenarioToStartAndGetItsId(question.UserId, ScenarioId, question.Priority, question.ScenarioType,
            question.Date, question.SprintNumber, question.SprintReplyNumber, question.IsDelayed);

        await messageSender.TrySay(EveningStandUpMessages.AskReady(scenarioToStartId, question.IsDelayed), question.UserId);

        return true;
    }

    private async Task<bool> TryStart(string text)
    {
         var splittedText = text.Split();

        if (splittedText.Length != 2) 
            return false;

        var eveningStandupCommand = splittedText[0];
        var scenarioToStartId = splittedText[1];

        if (eveningStandupCommand != CommandsConstants.StartEveningStandupActionPrefix)
            return false;

        var scenarioToStart = await scenariosToStartRepository.GetScenarioToStart(scenarioToStartId);
        if (scenarioToStart is null || scenarioToStart.ScenarioType is not SelfScenarioType)
            return false;
        await scenariosToStartRepository.DeleteScenarioToStart(scenarioToStartId);
        
        await scenarioStateRepository.StartNewScenario(scenarioToStart.ChatId, ScenarioId,
            scenarioToStart.Date, scenarioToStart.SprintNumber, scenarioToStart.SprintReplyNumber);

        await messageSender.TrySay(EveningStandUpMessages.AskWinning(), scenarioToStart.ChatId);
        await scenarioStateRepository.UpdateData(scenarioToStart.ChatId,
            new RegularScenarioData() {AnswerType = AnswerType.EveningStandUpWinnings});

        return true;
    }

    public async Task<bool> TryHandle(TelegramEvent telegramEvent, CurrentScenarioInfo? scenarioInfo)
    {
        var chatId = telegramEvent.ChatId;

        var text = telegramEvent.Text?.Trim() ?? "";
        var tryStartScenarioResult = await TryStart(text);

        if (tryStartScenarioResult)
            return true;

        if (scenarioInfo == null || scenarioInfo.ScenarioId != ScenarioId)
            return false;

        var scenarioData = JsonConvert.DeserializeObject<RegularScenarioData>(scenarioInfo.Data);
        if (scenarioData == null)
            throw new ArgumentException("Не правильно указана специальная информация для сценария вечерней рефлексии");

        if (text == CommandsConstants.StartCheckupRoutineActions)
            return false;
        
        if (text == CommandsConstants.FinishEveningStandUp)
        {
            await messageSender.TrySay(EveningStandUpMessages.Finish(), chatId);
            await scenarioStateRepository.EndScenarioNoMatterWhat(chatId);
            return true;
        }

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

        if (scenarioData.AnswerType == AnswerType.EveningStandUpWinnings)
        {
            await messageSender.TrySay(EveningStandUpMessages.AskLive(), chatId);
            await scenarioStateRepository.UpdateData(chatId,
                new RegularScenarioData() {AnswerType = AnswerType.EveningStandUpLive});
        }

        if (scenarioData.AnswerType == AnswerType.EveningStandUpLive)
        {
            await messageSender.TrySay(EveningStandUpMessages.AskPleasure(), chatId);
            await scenarioStateRepository.UpdateData(chatId,
                new RegularScenarioData() {AnswerType = AnswerType.EveningStandUpPleasure});
        }

        if (scenarioData.AnswerType == AnswerType.EveningStandUpPleasure)
        {
            await messageSender.TrySay(EveningStandUpMessages.AskDrive(), chatId);
            await scenarioStateRepository.UpdateData(chatId,
                new RegularScenarioData() {AnswerType = AnswerType.EveningStandUpDrive});
        }

        if (scenarioData.AnswerType == AnswerType.EveningStandUpDrive)
        {
            await messageSender.TrySay(EveningStandUpMessages.AskStartCheckUpRoutineActions(), chatId);
            await scenarioStateRepository.UpdateData(chatId, new RegularScenarioData());
        }


        return true;
    }
}