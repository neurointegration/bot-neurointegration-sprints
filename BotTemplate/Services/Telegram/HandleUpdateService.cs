using BotTemplate.Services.S3Storage;
using BotTemplate.Services.Telegram.Commands;
using BotTemplate.Services.YDB;
using BotTemplate.Services.YDB.Repo;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotTemplate.Services.Telegram;

public class HandleUpdateService
{
    private readonly IMessageView messageView;
    private readonly IChatCommandHandler[] commands;
    private readonly IMessageDetailsBucket messageDetailsBucket;
    private readonly IBotDatabase botDatabase;

    public HandleUpdateService(
        IMessageView messageView, 
        IChatCommandHandler[] commands,
        IMessageDetailsBucket messageDetailsBucket,
        IBotDatabase botDatabase)
    {
        this.messageView = messageView;
        this.commands = commands;
        this.messageDetailsBucket = messageDetailsBucket;
        this.botDatabase = botDatabase;
    }

    public async Task Handle(Update update)
    {
        var messageDateTable = await CurrentScenarioRepo.InitWithDatabase(botDatabase);
        var handler = update.Type switch
        {
            UpdateType.Message => HandleMessage(update.Message!, messageDateTable),
            _ => HandleDefaultUpdate()
        };

        await handler;
    }

    private async Task HandleMessage(Message message, CurrentScenarioRepo currentScenarioRepo)
    {
        await messageDetailsBucket.AddMessage(message.Chat.Id, message);
        if (message.Type == MessageType.Text)
        {
            await HandlePlainText(message.Text!, message.Chat.Id, currentScenarioRepo);
            currentScenarioRepo.UpdateOrInsertDateTime(message.Chat.Id);
            return;
        }

        await HandleNonCommandMessage(message.Chat.Id, currentScenarioRepo);
        currentScenarioRepo.UpdateOrInsertDateTime(message.Chat.Id);
    }

    private async Task HandlePlainText(string text, long fromChatId, CurrentScenarioRepo currentScenarioRepo)
    {
        var command = commands.FirstOrDefault(c => text.StartsWith(c.Command));

        if (command is null)
        {
            await HandleNonCommandMessage(fromChatId, currentScenarioRepo);
            return;
        }

        await command.HandlePlainText(text, fromChatId);
    }

    private async Task HandleNonCommandMessage(long fromChatId, CurrentScenarioRepo currentScenarioRepo)
    {
        await messageView.Say(
            await GetGreetingMessage(fromChatId, currentScenarioRepo),
            fromChatId
        );
    }
    
    private Task HandleDefaultUpdate()
    {
        return Task.CompletedTask;
    }

    private async Task<string> GetGreetingMessage(long fromChatId, CurrentScenarioRepo currentScenarioRepo)
    {
        var lastMessageDateTime = await currentScenarioRepo.FindLastMessageDateTime(fromChatId);
        var separationInterval = DateTime.Now - lastMessageDateTime;
        var usersOverWeek = await currentScenarioRepo.GetPastWeekUsersCount();

        var peopleCountPlural = ((long) usersOverWeek).PluralizeLong(
            "человеком|людьми|людьми"
        );

        var dayCountPlural = separationInterval is null
            ? "никогда"
            : separationInterval.Value.Days.Pluralize(
                "день|дня|дней"
            );

        return $"Давно не виделись! А именно {dayCountPlural}! " +
               $"Я пообщался уже с {peopleCountPlural} за эту неделю.";
    }
}