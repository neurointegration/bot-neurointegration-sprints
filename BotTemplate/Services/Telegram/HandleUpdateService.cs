using BotTemplate.Services.S3Storage;
using BotTemplate.Services.Telegram.Commands;
using BotTemplate.Services.YDB;
using BotTemplate.Services.YDB.Repo;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotTemplate.Services.Telegram;

public class HandleUpdateService
{
    private readonly IMessageView _messageView;
    private readonly IBotDatabase _botDatabase;
    
    private IChatCommandHandler[] _commands;
    private CurrentScenarioRepo _currentScenarioRepo;
    private UserAnswersRepo _userAnswersRepo;

    public HandleUpdateService(
        IMessageView messageView,
        IBotDatabase botDatabase)
    {
        _messageView = messageView;
        _botDatabase = botDatabase;
    }

    public async Task Handle(Update update)
    {
        var scenariosRepo = await ScenariosRepo.InitWithDatabase(_botDatabase);
        _currentScenarioRepo = await CurrentScenarioRepo.InitWithDatabase(_botDatabase, scenariosRepo);
        _userAnswersRepo = await UserAnswersRepo.InitWithDatabase(_botDatabase);
        
        _commands = new IChatCommandHandler[]
        {
            new StartCommandHandler(_currentScenarioRepo),
            new GetAnswersCommandHandler(_userAnswersRepo)
        };
        
        var handler = update.Type switch
        {
            UpdateType.Message => HandleMessage(update.Message!),
            _ => HandleDefaultUpdate(update.Message!.Chat.Id)
        };

        await handler;
    }

    private async Task HandleMessage(Message message)
    {
        if (message.Type == MessageType.Text)
        {
            await HandlePlainText(message.Text!, message.Chat.Id);
            return;
        }

        await HandleNonCommandMessage(message.Chat.Id, message.Text!);
    }

    private async Task HandlePlainText(string text, long fromChatId)
    {
        var command = _commands.FirstOrDefault(c => text.StartsWith(c.Command));

        if (command is null)
        {
            await HandleNonCommandMessage(fromChatId, text);
            return;
        }

        await HandleCommandMessage(fromChatId, command);
    }
    
    private async Task HandleCommandMessage(long fromChatId, IChatCommandHandler command)
    {
        var scenarioId = await _currentScenarioRepo.GetScenarioIdByChatId(fromChatId);
        if (scenarioId is not null)
            await SendMessage(fromChatId, "Ответьте на вопросы, прежде чем вызывать новую команду");
        else
        {
            var message = await command.HandlePlainText(fromChatId);
            await SendMessage(fromChatId, message);
            
        }
    }

    private async Task HandleNonCommandMessage(long fromChatId, string text)
    {
        var key = await _currentScenarioRepo.GetCurrentKey(fromChatId);
        if (key == null) 
            return;
        
        await _userAnswersRepo.SaveAnswer(fromChatId, key, text);
        var message = await _currentScenarioRepo.IncreaseAndGetNewMessage(fromChatId);

        await SendMessage(fromChatId, message);
    }
    
    private async Task HandleDefaultUpdate(long fromChatId)
    {
        await SendMessage(fromChatId, "Я пока не умею поддерживать стикеры, картинки и прочие нетекстовые сообщения!");
    }

    private async Task SendMessage(long fromChatId, string? text)
    {
        if (text is null)
            return;
        
        await _messageView.Say(text, fromChatId);
    }
}