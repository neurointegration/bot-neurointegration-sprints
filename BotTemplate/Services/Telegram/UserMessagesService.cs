using System.Text.RegularExpressions;
using BotTemplate.Extensions;
using BotTemplate.Models.ScenariosData;
using BotTemplate.Models.Telegram;
using BotTemplate.Scenarios;
using BotTemplate.Scenarios.RegularScenarios;
using BotTemplate.Services.YDB;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotTemplate.Services.Telegram;

public class UserMessagesService
{
    private readonly IMessageSender messageSender;
    private readonly IEnumerable<IScenario> scenarios;
    private readonly IEnumerable<IRegularScenario> regularScenarios;
    private readonly string telegramBotUrl;
    private readonly HttpClient client = new();

    private readonly ScenarioStateRepository scenarioStateRepository;
    private readonly ScenariosToStartRepository scenariosToStartRepository;

    public UserMessagesService(
        IMessageSender messageSender,
        IEnumerable<IScenario> scenarios,
        IEnumerable<IRegularScenario> regularScenarios,
        ScenarioStateRepository scenarioStateRepository,
        ScenariosToStartRepository scenariosToStartRepository,
        Configuration configuration)
    {
        this.messageSender = messageSender;
        this.scenarios = scenarios;
        this.regularScenarios = regularScenarios;
        this.scenarioStateRepository = scenarioStateRepository;
        this.scenariosToStartRepository = scenariosToStartRepository;

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
        string[] splittedReadyQuery;

        if (Regex.IsMatch(text, CommandsConstants.StartRegularScenarioRegexPattern)) 
        {
            splittedReadyQuery = text.Split();
            var scenarioToStartId = splittedReadyQuery[1];
            var scenarioToStart = await scenariosToStartRepository.GetScenarioToStartAndDeleteIt(scenarioToStartId);

            await TryStartRegularScenario(scenarioToStart);
            if (splittedReadyQuery.Length != 3)
                return;
            
            var newText = splittedReadyQuery[2];
            telegramEvent = new TelegramEvent
            {
                ChatId = telegramEvent.ChatId,
                Text = newText,
                Username = telegramEvent.Username,
                MessageType = telegramEvent.MessageType
            };
            scenarioInfo = ScenarioToStartExtensions.ToCurrentScenarioInfo(scenarioToStart);
        }

        foreach (var scenario in scenarios)
        {
            success = await scenario.TryHandle(telegramEvent, scenarioInfo);
            if (success)
                break;
        }
        
        if (!success)
            await HandleUnknownCommand(telegramEvent.ChatId);
    }

    private async Task TryStartRegularScenario(ScenarioToStart? scenarioToStart) 
    {
        if (scenarioToStart is null)
        {
            return;
        }

        foreach (var regularScenario in regularScenarios)
        {
            var success = await regularScenario.Start(scenarioToStart);
            if (success)
                break;
        }
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