using Common.Ydb;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Neurointegration.Api.DI;
using Neurointegration.Api.Settings;
using Neurointegration.Api.Storages.Tables;
using Yandex.Cloud.Functions;

namespace BotTemplate;

public class DbMigrateHandler : YcFunction<string, Response>
{
    public Response FunctionHandler(string request, Context context)
    {
        Handle().Wait();
        return new Response(200, "ok");
    }

    public async Task Handle()
    {
        var secretSettings = ApiSecretSettings.FromEnvironment();
        using var factory = LoggerFactory.Create(builder => builder.AddSimpleConsole());
        var service = new ServiceCollection()
            .AddSingleton<ILogger>(factory.CreateLogger("Migrate"))
            .AddTransient(provider =>
                new YdbClient(secretSettings.YdbSecretSettings, provider.GetRequiredService<ILogger>()))
            .AddInitialize();
        var serviceProvider = service.BuildServiceProvider();
        var initializer = serviceProvider.GetService<YdbInitializer>() ??
                          throw new ArgumentException("Нет экземпляра инициализатора бд для апи");
        await initializer.CreateTables();
    }
}