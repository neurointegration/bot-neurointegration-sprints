using BotTemplate.Models.Telegram;
using BotTemplate.Scenarios.Common.Messages;
using BotTemplate.Services.Telegram;

namespace BotTemplate.Scenarios.Common;

public class MainRoutineActionsScenario : IScenario
{
    private readonly IMessageSender messageSender;
    private const string Command = CommandsConstants.RoutineActionsCommand;

    public MainRoutineActionsScenario(IMessageSender messageSender)
    {
        this.messageSender = messageSender;
    }

    public async Task<bool> TryHandle(TelegramEvent telegramEvent, CurrentScenarioInfo? scenarioInfo)
    {
        var text = telegramEvent.Text ?? "";
        if (text != Command)
            return false;

        await messageSender.TrySay(ShowMainRoutineActionsMessages.GetMessage(), telegramEvent.ChatId);
        return true;
    }
}