using BotTemplate.Client;
using BotTemplate.DI;
using BotTemplate.Services.Telegram;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Telegram.Bot.Types;
using Yandex.Cloud.Functions;

namespace BotTemplate;

public class TelegramHandler : YcFunction<string, Response>
{
    public Response FunctionHandler(string request, Context context)
    {
        try
        {
            HandleRequest(request, context).Wait();
            return new Response(200, "ok");
        }
        catch (Exception e)
        {
            return new Response(500, $"Error {e}");
        }
    }

    private async Task HandleRequest(string request, Context context)
    {
        var configuration = Configuration.FromEnvironment();
        var service = new ServiceCollection();
        var provider = service.BuildDeps(configuration, "TelegramHandler");

        var userMessagesService = provider.GetRequiredService<UserMessagesService>();

        var body = JObject.Parse(request).GetValue("body")!.Value<string>()!;
        var message = JsonConvert.DeserializeObject<Update>(body)!;

        await userMessagesService.HandleMessage(message);
    }
}