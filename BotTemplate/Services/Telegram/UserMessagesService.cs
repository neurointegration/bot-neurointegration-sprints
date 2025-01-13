using BotTemplate.Models.Telegram;
using BotTemplate.Scenarios;
using BotTemplate.Services.YDB;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotTemplate.Services.Telegram;

public class UserMessagesService
{
    private readonly IMessageSender messageSender;
    private readonly IEnumerable<IScenario> scenarios;
    private readonly string telegramBotUrl;
    private readonly HttpClient client = new();

    private readonly ScenarioStateRepository scenarioStateRepository;

    public UserMessagesService(
        IMessageSender messageSender,
        IEnumerable<IScenario> scenarios,
        ScenarioStateRepository scenarioStateRepository,
        Configuration configuration)
    {
        this.messageSender = messageSender;
        this.scenarios = scenarios;
        this.scenarioStateRepository = scenarioStateRepository;

        telegramBotUrl = $"https://api.telegram.org/bot{configuration.TelegramToken}";
    }

    public async Task HandleMessage(Update message)
    {
        var telegramEvent = message.Type switch
        {
            UpdateType.Message => new TelegramEvent
            {
                ChatId = message.Message!.Chat.Id,
                Text = message.Message.Text!,
                Username = message.Message.From!.Username!,
                MessageType = message.Message.Type
            },
            UpdateType.CallbackQuery => new TelegramEvent
            {
                ChatId = message.CallbackQuery!.Message!.Chat.Id,
                Text = message.CallbackQuery.Data!,
                Username = message.CallbackQuery.From.Username!,
                MessageType = message.CallbackQuery.Message.Type,
            },
            _ => null
        };

        if (telegramEvent is null)
            await HandleDefaultUpdate(message.Message!.Chat.Id);
        else
            await HandleMessage(telegramEvent);

        if (message.Type is UpdateType.CallbackQuery)
            await client.GetAsync(
                $"{telegramBotUrl}/answerCallbackQuery?callback_query_id={message.CallbackQuery!.Id}");
    }

    private async Task HandleMessage(TelegramEvent telegramEvent)
    {
        var chatId = telegramEvent.ChatId;
        var text = telegramEvent.Text;
        var scenarioInfo = await scenarioStateRepository.GetInfoByChatId(chatId);

        if (text == null)
        {
            await HandleUnknownCommand(telegramEvent.ChatId);
            return;
        }

        var success = false;

        foreach (var scenario in scenarios)
        {
            success = await scenario.TryHandle(telegramEvent, scenarioInfo);
            if (success)
                break;
        }
        
        if (!success)
            await HandleUnknownCommand(telegramEvent.ChatId);
    }

    private async Task HandleDefaultUpdate(long chatId)
    {
        await messageSender.Say(MessageConstants.DefaultText, chatId);
    }
    private async Task HandleUnknownCommand(long chatId)
    {
        await messageSender.Say(MessageConstants.UnknownCommand, chatId);
    }
    
}