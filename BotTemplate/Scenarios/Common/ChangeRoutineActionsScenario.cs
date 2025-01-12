using BotTemplate.Client;
using BotTemplate.Models.ScenariosData;
using BotTemplate.Models.Telegram;
using BotTemplate.Services.Telegram;
using BotTemplate.Services.Telegram.Messages.Register;
using BotTemplate.Services.YDB;
using Neurointegration.Api.DataModels.Models;
using Newtonsoft.Json;

namespace BotTemplate.Scenarios.Common;

public class ChangeRoutineActionsScenario : IScenario
{
    private readonly IBackendApiClient backendApiClient;
    private readonly IMessageSender messageSender;
    private readonly ScenarioStateRepository scenarioStateRepository;
    private const string ScenarioId = CommandsConstants.ChangeRoutineActions;

    private readonly HashSet<string> commands = new()
        {CommandsConstants.ChangeRoutineActions, CommandsConstants.ReturnToRoutineActionsList};

    private readonly Dictionary<string, RoutineType> addRoutineCommands;

    public ChangeRoutineActionsScenario(
        IBackendApiClient backendApiClient,
        IMessageSender messageSender,
        ScenarioStateRepository scenarioStateRepository)
    {
        this.backendApiClient = backendApiClient;
        this.messageSender = messageSender;
        this.scenarioStateRepository = scenarioStateRepository;
        addRoutineCommands = Enum.GetValues<RoutineType>()
            .Select(type => (type, command: CommandsConstants.AddRoutineAction(type)))
            .ToDictionary(x => x.command, x => x.type);
    }

    public async Task<bool> TryHandle(TelegramEvent telegramEvent, CurrentScenarioInfo? scenarioInfo)
    {
        var text = telegramEvent.Text?.Trim();
        if (!commands.Contains(text) && scenarioInfo?.ScenarioId != ScenarioId)
            return false;

        var chatId = telegramEvent.ChatId;
        var routineActions = await backendApiClient.GetUserRoutineActions(chatId);

        if (text == CommandsConstants.ChangeRoutineActions)
        {
            var messageId =
                await messageSender.TrySay(RoutineMessage.CurrentRoutines(routineActions), chatId);
            var data = new RoutineActionsScenarioData()
            {
                MessageId = messageId.Value
            };
            await scenarioStateRepository.StartNewScenarioAndGetMessage(chatId, ScenarioId,
                data: JsonConvert.SerializeObject(data));
            return true;
        }

        if (scenarioInfo == null || scenarioInfo.Data == null)
            return false;

        var scenarioData = JsonConvert.DeserializeObject<RoutineActionsScenarioData>(scenarioInfo.Data);
        if (scenarioData == null)
            throw new ArgumentException("Не правильно указана специальная информация для сценария рутинных действий");

        if (text == CommandsConstants.ReturnToRoutineActionsList)
        {
            await messageSender.TryEdit(RoutineMessage.CurrentRoutines(routineActions),
                chatId,
                scenarioData.MessageId);
            return true;
        }

        if (addRoutineCommands.TryGetValue(text ?? "", out var routineType))
        {
            await messageSender.TryEdit(RoutineMessage.AddRoutinesMessage(routineType),
                chatId,
                scenarioData.MessageId);
            await scenarioStateRepository.UpdateData(chatId, scenarioData with {RoutineType = routineType});
            return true;
        }

        if (text?.StartsWith(CommandsConstants.DeleteRoutineActionPrefix) ?? false)
        {
            var index = int.Parse(text.Replace(CommandsConstants.DeleteRoutineActionPrefix, ""));
            var deleteAction = routineActions[index];

            await backendApiClient.DeleteAction(chatId, deleteAction.ActionId);
            routineActions = await backendApiClient.GetUserRoutineActions(chatId);
            await messageSender.TryEdit(RoutineMessage.CurrentRoutines(routineActions), chatId, scenarioData.MessageId);
            return true;
        }

        if (scenarioData.RoutineType != null)
        {
            var newActions = text!.Split("\n")
                .Select(action => new RoutineAction(scenarioData.RoutineType.Value, action.Trim()))
                .ToList();

            await backendApiClient.AddActions(chatId, newActions);
            routineActions = await backendApiClient.GetUserRoutineActions(chatId);
            await messageSender.TryEdit(RoutineMessage.CurrentRoutines(routineActions), chatId, scenarioData.MessageId);
            await scenarioStateRepository.UpdateData(chatId, scenarioData with {RoutineType = null});
            return true;
        }

        if (text == CommandsConstants.CancelEditRoutineActions)
        {
            await messageSender.TryEdit(RoutineMessage.FinishChangeRoutineActions(), chatId, scenarioData.MessageId);
            await scenarioStateRepository.EndScenarioNoMatterWhat(chatId);
            return true;
        }

        return false;
    }
}