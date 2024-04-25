using BotTemplate.Services.YDB;
using BotTemplate.Services.YDB.Repo;
using Telegram.Bot.Types;

namespace BotTemplate.Services.Telegram.Commands;

public interface IChatCommandHandler
{
    string Command { get; }
    
    Task<string?> HandlePlainText(long fromChatId);
}