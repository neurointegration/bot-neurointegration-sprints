using BotTemplate.Client;
using BotTemplate.Models.Telegram;
using BotTemplate.Services.Telegram;
using BotTemplate.Services.YDB;
using Grpc.Core.Logging;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Yandex.Cloud.Functions;

namespace BotTemplate;

public class TriggerHandler : YcFunction<string, Response>
{
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
        var view = new HtmlMessageView(tgClient);
        var botDatabase = new BotDatabase(configuration);
        var backendApiClient = InitializeLocalClient.Init().GetService<IBackendApiClient>() ?? throw new ArgumentException("Не задан клиент бэкенда");
        var triggerFrequencyMinutes = int.Parse(configuration.TriggerFrequencyMinutes!);

        var updateService = new TriggerService(view, botDatabase, backendApiClient, telegramBotUrl, triggerFrequencyMinutes);
        await updateService.Handle();
    }
}