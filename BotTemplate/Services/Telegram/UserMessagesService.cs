using BotTemplate.Models.Telegram;
using BotTemplate.Scenarios.Coach;
using BotTemplate.Scenarios.RegularScenarios;
using BotTemplate.Scenarios.User;
using BotTemplate.Services.YDB;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotTemplate.Services.Telegram;

public class UserMessagesService
{
    private readonly IMessageSender messageSender;
    private readonly RegisterScenario registerScenario;
    private readonly SettingsScenario settingsScenario;
    private readonly GetStudentsScenario getStudentsScenario;
    private readonly GetTablesLinksScenario getTablesLinksScenario;
    private readonly string telegramBotUrl;
    private readonly HttpClient client = new();

    private readonly ScenarioStateRepository scenarioStateRepository;
    private readonly StatusScenario statusScenario;
    private readonly EveningStandUpScenario eveningStandUpScenario;
    private readonly WeekendReflectionScenario weekendReflectionScenario;

    public UserMessagesService(
        IMessageSender messageSender,
        RegisterScenario registerScenario,
        SettingsScenario settingsScenario,
        GetStudentsScenario getStudentsScenario,
        GetTablesLinksScenario getTablesLinksScenario,
        ScenarioStateRepository scenarioStateRepository,
        StatusScenario statusScenario,
        EveningStandUpScenario eveningStandUpScenario,
        WeekendReflectionScenario weekendReflectionScenario)
    {
        this.messageSender = messageSender;
        this.registerScenario = registerScenario;
        this.settingsScenario = settingsScenario;
        this.getStudentsScenario = getStudentsScenario;
        this.getTablesLinksScenario = getTablesLinksScenario;
        this.scenarioStateRepository = scenarioStateRepository;
        this.statusScenario = statusScenario;
        this.eveningStandUpScenario = eveningStandUpScenario;
        this.weekendReflectionScenario = weekendReflectionScenario;

        telegramBotUrl = $"https://api.telegram.org/bot{Configuration.FromEnvironment().TelegramToken}";
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
                MessageType = message.CallbackQuery.Message.Type
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
        var scenarioId = await scenarioStateRepository.GetScenarioIdByChatId(chatId);

        if (text == null)
        {
            await HandleDefaultUpdate(telegramEvent.ChatId);
            return;
        }


        if (text == "/start" || scenarioId == registerScenario.ScenarioId)
        {
            await registerScenario.Handle(telegramEvent);
            return;
        }

        if (text == "Настройки" || scenarioId == settingsScenario.ScenarioId)
        {
            if (scenarioId != null && scenarioId != settingsScenario.ScenarioId)
                await messageSender.Say("Закночи другой сценарий, прежде чем открыть настройки.", chatId);
            else
                await settingsScenario.Handle(telegramEvent);

            return;
        }

        if (text == "Таблица результатов")
        {
            if (scenarioId != null)
                await messageSender.Say("Закночи другой сценарий, прежде чем получать таблицу результатов.", chatId);
            else
                await getTablesLinksScenario.Handle(telegramEvent);

            return;
        }

        if (text == "Мои ученики")
        {
            if (scenarioId != null)
                await messageSender.Say(
                    "Закночи другой сценарий, прежде чем получать таблицы результатов твоих учеников.", chatId);
            else
                await getStudentsScenario.Handle(telegramEvent);

            return;
        }

        var key = await scenarioStateRepository.GetCurrentKey(chatId);
        if (key == null)
            return;

        var nonCommandScenarios = new List<IRegularScenario>()
        {
            statusScenario, eveningStandUpScenario, weekendReflectionScenario
        };
        foreach (var scenario in nonCommandScenarios)
        {
            await scenario.Handle(telegramEvent);
        }
    }

    private async Task HandleDefaultUpdate(long chatId)
    {
        await messageSender.Say(MessageConstants.DefaultText, chatId);
    }
}