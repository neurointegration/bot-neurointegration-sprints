using BotTemplate.Client;
using BotTemplate.Models.Telegram;
using BotTemplate.Scenarios.User.Settings;
using BotTemplate.Services.Telegram;

namespace BotTemplate.Scenarios.User;

public class SettingsScenario : IScenario
{
    private readonly IBackendApiClient backendApiClient;
    private readonly IMessageSender messageSender;

    private HashSet<string> commands = new HashSet<string>()
    {
        CommandsConstants.Settings,
        CommandsConstants.SettingsCommand
    };

    public SettingsScenario(
        IBackendApiClient backendApiClient,
        IMessageSender messageSender)
    {
        this.backendApiClient = backendApiClient;
        this.messageSender = messageSender;
    }

    public async Task<bool> TryHandle(TelegramEvent telegramEvent, CurrentScenarioInfo? scenarioInfo)
    {
        if (commands.Contains(telegramEvent.Text?.Trim().ToLower() ?? ""))
        {
            var chatId = telegramEvent.ChatId;

            var getUser = await backendApiClient.GetUser(chatId);
            if (!getUser.IsSuccess)
                await messageSender.Say(MessageConstants.UnknownErrorText, chatId);

            await messageSender.TrySay(ShowSettingsMessage.GetMessage(getUser.Value), chatId);
            return true;
        }

        return false;
    }
}