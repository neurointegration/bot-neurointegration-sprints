using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Message = BotTemplate.Models.Telegram.Message;

namespace BotTemplate.Services.Telegram;

public class HtmlMessageSender : IMessageSender
{
    private readonly ITelegramBotClient botClient;
    private readonly ILogger log;

    public HtmlMessageSender(ITelegramBotClient client, ILogger log)
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
            log.LogError($"Не удалось отправить сообщение {text} пользователю {chatId}. Ошибка {e.Message}");
        }
    }
    
    public async Task<int?> TrySay(Message message, long chatId)
    {
        try
        {
            var sendingMessage = await botClient.SendTextMessageAsync(
                chatId,
                message.Text,
                parseMode: ParseMode.Html,
                replyMarkup: message.ReplyMarkup
            );

            return sendingMessage.MessageId;
        }
        catch (Exception e)
        {
            log.LogError($"Не удалось отправить сообщение {message.Text} пользователю {chatId}. Ошибка {e.Message}");
        }

        return null;
    }
    
    public async Task TryEdit(Message message, long chatId, int messageId)
    {
        if (message.ReplyMarkup is not InlineKeyboardMarkup inlineKeyboardMarkup)
            return;
        
        try
        {
            await botClient.EditMessageTextAsync(
                chatId,
                messageId,
                message.Text,
                parseMode: ParseMode.Html,
                replyMarkup: inlineKeyboardMarkup
            );
        }
        catch (Exception e)
        {
            log.LogError($"Не удалось изменить сообщение {messageId}. Ошибка {e.Message}");
        }
    }

    public async Task TrySay(string? text, long chatId)
    {
        if (text == null)
            return;
        
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
            log.LogError($"Не удалось отправить сообщение {text} пользователю {chatId}. Ошибка {e.Message}");
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
            log.LogError($"Не удалось отправить сообщение {text} пользователю {chatId}. Ошибка {e.Message}");
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