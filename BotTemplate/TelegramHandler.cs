using BotTemplate.Services.Telegram;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Telegram.Bot.Types;
using Yandex.Cloud.Functions;

namespace BotTemplate;

public class TelegramHandler : BaseFunctionHandler<UserMessagesService>
{
    protected override string LogCategoryName { get; set; } = "TelegramHandler";

    protected override async Task<string> InnerHandleRequest(string request, Context context)
    {
        try {
            var body = JObject.Parse(request).GetValue("body")!.Value<string>()!;
            var message = JsonConvert.DeserializeObject<Update>(body)!;

            await HandleService.HandleMessage(message);

            return "ok";
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Что-то пошло не так...");
            return "not ok";
        }
    }
}