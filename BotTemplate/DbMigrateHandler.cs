using BotTemplate.Services.YDB;
using BotTemplate.Services.YDB.Repo;
using Microsoft.Extensions.DependencyInjection;
using Neurointegration.Api.DI;
using Neurointegration.Api.Settings;
using Neurointegration.Api.Storages;
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
        var configuration = Configuration.FromEnvironment();
        var botDatabase = new BotDatabase(configuration);
        var scenariosRepo = await ScenariosRepository.InitWithCreate(botDatabase);
        await CurrentScenarioRepository.InitWithCreate(botDatabase, scenariosRepo);
        await UserAnswersRepository.InitWithCreate(botDatabase);
        await UsersRepository.InitWithCreate(botDatabase);

        var secretSettings = ApiSecretSettings.FromEnvironment();
        var service = new ServiceCollection()
            .AddTransient(_ => new YdbClient(secretSettings))
            .AddDb();
        var serviceProvider = service.BuildServiceProvider();
        var initializer = serviceProvider.GetService<YdbInitializer>() ??
                          throw new ArgumentException("Нет экземпляра инициализатора бд для апи");
        await initializer.CreateTables();
    }
}