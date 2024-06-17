using Telegram.Bot.Types.ReplyMarkups;

namespace BotTemplate.Models.Telegram;

public class Message
{
    public string Text { get; private set; }
    public IReplyMarkup? ReplyMarkup { get; private set; }

    public Message(string text, IReplyMarkup? replyMarkup = null)
    {
        Text = text;
        ReplyMarkup = replyMarkup;
    }
}