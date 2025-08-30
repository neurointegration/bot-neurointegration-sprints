using BotTemplate.Client;
using BotTemplate.Models.Telegram;
using BotTemplate.Scenarios.User.Settings;
using BotTemplate.Services.Telegram;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Net.Mime.MediaTypeNames;

namespace BotTemplate.Scenarios.User;

public class LoginScenario : IScenario
{
    private readonly IBackendApiClient backendApiClient;
    private readonly IMessageSender messageSender;
    private readonly string loginUrl;
    private readonly string? loginBotUsername;

    private HashSet<string> commands = new HashSet<string>()
    {
        CommandsConstants.Login,
        CommandsConstants.LoginCommand
    };

    public LoginScenario(
        IBackendApiClient backendApiClient,
        IMessageSender messageSender,
        Configuration configuration)
    {
        this.backendApiClient = backendApiClient;
        this.messageSender = messageSender;
        loginUrl = configuration.LoginUrl;
        loginBotUsername = configuration.LoginBotUsername;
    }

    public async Task<bool> TryHandle(TelegramEvent telegramEvent, CurrentScenarioInfo? scenarioInfo)
    {
        if (commands.Contains(telegramEvent.Text?.Trim().ToLower() ?? ""))
        {
            var chatId = telegramEvent.ChatId;

            var getUser = await backendApiClient.GetUser(chatId);
            if (!getUser.IsSuccess)
                await messageSender.Say(MessageConstants.UnknownErrorText, chatId);

            var loginButton = InlineKeyboardButton.WithLoginUrl("Авторизоваться",new LoginUrl
            {
                Url = loginUrl,
                BotUsername = loginBotUsername
            });

            var inline = new InlineKeyboardMarkup(loginButton);

            var loginMessage = new Models.Telegram.Message("Для авторизации на сайте Нейроспринт нажмите кнопку ниже", inline);

            await messageSender.TrySay(loginMessage, chatId);
            return true;
        }

        return false;
    }
}