using BotTemplate.Client;
using BotTemplate.Services.Telegram;
using BotTemplate.Services.YDB;
using Grpc.Core.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;
using Yandex.Cloud.Functions;

namespace BotTemplate;

public class TelegramHandler : YcFunction<string, Response>
{
    private const string CodePath = "/function/code/";

    public Response FunctionHandler(string request, Context context)
    {
        var logger = new ConsoleLogger();
        logger.ForType<string>().Info(request);
        try
        {
            HandleRequest(request, context).Wait();
            return new Response(200, "ok");
        }
        catch (Exception e)
        {
            logger.ForType<Exception>().Error(e, "Error");
            return new Response(500, $"Error {e}");
        }
    }

    private async Task HandleRequest(string request, Context context)
    {
        var configuration = Configuration.FromEnvironment();
        var tgClient = new TelegramBotClient(configuration.TelegramToken);
        var telegramBotUrl = $"https://api.telegram.org/bot{configuration.TelegramToken}";
        var body = JObject.Parse(request).GetValue("body")!.Value<string>()!;
        var update = JsonConvert.DeserializeObject<Update>(body)!;
        var view = new HtmlMessageView(tgClient);
        var botDatabase = new BotDatabase(configuration);
        var backendApiClient = InitializeLocalClient.Init();

        var updateService = new HandleUpdateService(view, botDatabase, backendApiClient, telegramBotUrl);
        await updateService.Handle(update);
    }
}