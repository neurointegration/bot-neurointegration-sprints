using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
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

public class HtmlMessageView : IMessageView
{
    private readonly ITelegramBotClient botClient;
    private readonly ILogger log;

    public HtmlMessageView(ITelegramBotClient client, ILogger log)
    {
        botClient = client;
        this.log = log;
    }


    public async Task Say(string text, long chatId)
    {
        try
        {
            await botClient.SendTextMessageAsync(
                chatId,
                text,
                parseMode: ParseMode.Html
            );
        }
        catch (Exception e)
        {
            log.LogError($"Не удалось отправить сообщение пользователю {chatId}");
        }
    }

    public async Task SayWithMarkup(string text, long chatId, IReplyMarkup? replyMarkup)
    {
        try
        {
            await botClient.SendTextMessageAsync(
                chatId,
                text,
                parseMode: ParseMode.Html,
                replyMarkup: replyMarkup
            );
        }
        catch (Exception e)
        {
            log.LogError($"Не удалось отправить сообщение пользователю {chatId}");
        }
    }

    public async Task ShowHelp(long chatId)
    {
        await Say(
            "Это шаблон telegram-бота, поддерживающий <b>Yandex Cloud Function</b>!\n" +
                "Если он тебе нужен, то тогда тебе " +
                "<a href=\"https://github.com/BasedDepartment1/cloud-function-bot\">сюда</a>.",
            chatId
        );
    }

    public async Task SendFile(long chatId, byte[] content, string filename, string caption)
    {
        await botClient.SendDocumentAsync(
            chatId,
            InputFile.FromStream(new MemoryStream(content), filename),
            caption: EscapeForHtml(caption)
        );
    }

    public async Task SendPicture(long chatId, byte[] picture, string caption)
    {
        await botClient.SendPhotoAsync(
            chatId,
            InputFile.FromStream(new MemoryStream(picture)),
            caption: EscapeForHtml(caption)
        );
    }
    
    private static string EscapeForHtml(string text)
    {
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");
    }
}