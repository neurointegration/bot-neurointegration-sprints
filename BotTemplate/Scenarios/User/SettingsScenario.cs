using BotTemplate.Client;
using BotTemplate.Models.Telegram;
using BotTemplate.Services.Telegram;
using BotTemplate.Services.Telegram.Messages.Settings;

namespace BotTemplate.Scenarios.User;

public class SettingsScenario : IScenario
{
    private readonly IBackendApiClient backendApiClient;
    private readonly IMessageSender messageSender;
    private const string Command = CommandsConstants.Settings;

    public SettingsScenario(
        IBackendApiClient backendApiClient,
        IMessageSender messageSender)
    {
        this.backendApiClient = backendApiClient;
        this.messageSender = messageSender;
    }
    
    public async Task<bool> TryHandle(TelegramEvent telegramEvent, CurrentScenarioInfo? scenarioInfo)
    {
        if (telegramEvent.Text?.Trim().ToLower() == Command)
        {
            var chatId = telegramEvent.ChatId;
            
            var getUser = await backendApiClient.GetUser(chatId);
            if (!getUser.IsSuccess)
                await messageSender.Say(MessageConstants.UnknownErrorText, chatId);

            var message = ShowSettingsMessage.GetMessage(getUser.Value);
            await messageSender.SayWithMarkup(message.Text, chatId, message.ReplyMarkup);
            return true;
        }

        return false;
    }
}