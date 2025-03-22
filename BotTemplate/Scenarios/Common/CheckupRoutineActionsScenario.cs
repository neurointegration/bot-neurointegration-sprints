using BotTemplate.Client;
using BotTemplate.Models.ScenariosData;
using BotTemplate.Models.Telegram;
using BotTemplate.Services.Telegram;
using BotTemplate.Services.Telegram.Messages.Register;
using BotTemplate.Services.YDB;
using Newtonsoft.Json;

namespace BotTemplate.Scenarios.Common;

public class CheckupRoutineActionsScenario : IScenario
{
    private readonly IBackendApiClient backendApiClient;
    private readonly IMessageSender messageSender;
    private readonly ScenarioStateRepository scenarioStateRepository;
    private const string ScenarioId = CommandsConstants.StartCheckupRoutineActions;

    private readonly HashSet<string> commands = new()
        {CommandsConstants.StartCheckupRoutineActions, CommandsConstants.FinishCheckupRoutineActions};

    public CheckupRoutineActionsScenario(
        IBackendApiClient backendApiClient,
        IMessageSender messageSender,
        ScenarioStateRepository scenarioStateRepository)
    {
        this.backendApiClient = backendApiClient;
        this.messageSender = messageSender;
        this.scenarioStateRepository = scenarioStateRepository;
    }

    public async Task<bool> TryHandle(TelegramEvent telegramEvent, CurrentScenarioInfo? scenarioInfo)
    {
        var text = telegramEvent.Text?.Trim() ?? "";
        if (!commands.Contains(text) && scenarioInfo?.ScenarioId != ScenarioId)
            return false;

        var chatId = telegramEvent.ChatId;
        var routineActions = await backendApiClient.GetUserRoutineActions(chatId);

        if (text == CommandsConstants.StartCheckupRoutineActions)
        {
            var messageId = await messageSender.TrySay(RoutineMessage.RoutineWeekendStatus(routineActions), chatId);
            var data = new RoutineActionsScenarioData()
            {
                MessageId = messageId.Value
            };
            await scenarioStateRepository.StartNewScenario(chatId, ScenarioId,
                data: JsonConvert.SerializeObject(data));
            return true;
        }
        
        var scenarioData = JsonConvert.DeserializeObject<RoutineActionsScenarioData>(scenarioInfo.Data);
        if (scenarioData == null)
            throw new ArgumentException("Не правильно указана специальная информация для сценария рутинных действий");
        
        if (text.StartsWith(CommandsConstants.CheckupRoutineActionPrefix))
        {
            var actionId = text.Replace(CommandsConstants.CheckupRoutineActionPrefix, "");
            await backendApiClient.CheckupAction(chatId, actionId);
            routineActions = await backendApiClient.GetUserRoutineActions(chatId);
            await messageSender.TryEdit(RoutineMessage.RoutineWeekendStatus(routineActions), chatId, scenarioData.MessageId);
            return true;
        }
        
        if (text == CommandsConstants.FinishCheckupRoutineActions)
        {
            await messageSender.TryEdit(RoutineMessage.FinishCheckup(), chatId, scenarioData.MessageId);
            await scenarioStateRepository.EndScenarioNoMatterWhat(chatId);
            return true;
        }


        return false;
    }
}