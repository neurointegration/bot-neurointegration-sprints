using BotTemplate.Client;
using BotTemplate.Models.Telegram;
using BotTemplate.Services.Telegram;
using BotTemplate.Services.Telegram.Messages.Status;
using BotTemplate.Services.YDB;
using Microsoft.Extensions.Logging;
using Neurointegration.Api.DataModels.Dto;
using Neurointegration.Api.DataModels.Models;

namespace BotTemplate.Scenarios.RegularScenarios;

public abstract class BaseRegularScenario : IRegularScenario
{
    protected readonly ScenarioStateRepository ScenarioStateRepository;
    protected readonly ILogger Logger;
    protected readonly IMessageSender MessageSender;
    protected readonly IBackendApiClient BackendApiClient;
    private HashSet<IMessageCommand> messageCommands;
    protected virtual string ScenarioId { get; } = default!;
    public virtual ScenarioType ScenarioType { get; } = default!;

    public BaseRegularScenario(
        ScenarioStateRepository scenarioStateRepository,
        IMessageSender messageSender,
        IBackendApiClient backendApiClient,
        ILogger logger)
    {
        ScenarioStateRepository = scenarioStateRepository;
        Logger = logger;
        MessageSender = messageSender;
        BackendApiClient = backendApiClient;
        messageCommands = new HashSet<IMessageCommand>()
        {
            new HandleStateResponse(),
            new SendStateMessage()
        };
    }

    public virtual async Task Start(Question question)
    {
        if (question.ScenarioType != ScenarioType)
            return;

        Logger.LogInformation($"Начало сценария `{ScenarioType.ToString()}` для пользователя {question.UserId}");
        var message = await ScenarioStateRepository.StartNewScenarioAndGetMessage(question.UserId, ScenarioId,
            question.Date, question.SprintNumber, question.SprintReplyNumber);

        if (!await TryProcessCommand(message, question.UserId, null))
            await MessageSender.TrySay(message, question.UserId);
    }

    public virtual async Task Handle(TelegramEvent telegramEvent)
    {
        var chatId = telegramEvent.ChatId;
        var currentScenarioInfo = await ScenarioStateRepository.GetInfoByChatId(chatId);
        if (currentScenarioInfo.ScenarioId != ScenarioId)
            return;

        var text = telegramEvent.Text ?? "";
        Logger.LogInformation(
            $"Ответ {currentScenarioInfo!.Index!.Value} на `{ScenarioType.ToString()}` для пользователя {chatId}");

        var sendAnswer = new SendAnswer
        {
            UserId = chatId,
            Answer = text,
            AnswerNumber = currentScenarioInfo.Index!.Value,
            ScenarioType = ScenarioType,
            Date = DateOnly.FromDateTime(currentScenarioInfo.Date ?? DateTime.UtcNow),
            SprintNumber = (int) currentScenarioInfo.CurrentSprintNumber!.Value,
            SprintReplyNumber = currentScenarioInfo.SprintReplyNumber!.Value
        };
        await BackendApiClient.SendAnswerAsync(sendAnswer);

        var message = await ScenarioStateRepository.IncreaseAndGetNewMessage(chatId);
        if (!await TryProcessCommand(message, chatId, text))
            await MessageSender.TrySay(message, chatId);

        await ScenarioStateRepository.TryEndScenario(chatId);
    }

    private async Task<bool> TryProcessCommand(string? newMessage, long userId, string? userAnswer)
    {
        if (newMessage != null && newMessage.StartsWith('/'))
        {
            var messageCommand =
                messageCommands.FirstOrDefault(messageCommand => newMessage.StartsWith(messageCommand.Command));
            var result = await messageCommand!.Handle(MessageSender, userId, userAnswer);
            if (!result.IsSuccessful())
                await ScenarioStateRepository.DecreaseIndex(userId);
            return true;
        }

        return false;
    }
}