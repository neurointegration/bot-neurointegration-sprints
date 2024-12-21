using Telegram.Bot.Types.ReplyMarkups;

namespace BotTemplate.Services.Telegram;

public interface IMessageView
{
    Task Say(string text, long chatId);
    Task SayWithMarkup(string text, long chatId, IReplyMarkup? replyMarkup);
    Task ShowHelp(long chatId);
    Task SendFile(long chatId, byte[] content, string filename, string caption);
    Task SendPicture(long chatId, byte[] picture, string caption);
}