using BotTemplate.Client;
using BotTemplate.Scenarios.Coach;
using BotTemplate.Scenarios.RegularScenarios;
using BotTemplate.Scenarios.User;
using BotTemplate.Services.Telegram;
using BotTemplate.Services.YDB;
using BotTemplate.Services.YDB.Repo;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace BotTemplate.DI;

public static class InternalDependenciesExtensions
{
    public static IServiceProvider BuildDeps(
        this IServiceCollection serviceCollection,
        Configuration configuration,
        string categoryName)
    {
        using var factory = LoggerFactory.Create(builder => builder.AddSimpleConsole());

        return serviceCollection
            .AddSingleton<ILogger>(factory.CreateLogger(categoryName))
            .AddBackend()
            .AddTgClient(configuration.TelegramToken)
            .AddMessageSender()
            .AddBotDb(configuration)
            .AddRepositories()
            .AddScenarios()
            .AddSingleton<QuestionService>()
            .AddSingleton<UserMessagesService>()
            .BuildServiceProvider();
    }

    public static IServiceCollection AddTgClient(this IServiceCollection serviceCollection, string token)
    {
        serviceCollection.AddSingleton<ITelegramBotClient>(new TelegramBotClient(token));

        return serviceCollection;
    }

    public static IServiceCollection AddMessageSender(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IMessageSender>(provider =>
            new HtmlMessageSender(provider.GetRequiredService<ITelegramBotClient>(),
                provider.GetRequiredService<ILogger>()));

        return serviceCollection;
    }

    public static IServiceCollection AddBotDb(this IServiceCollection serviceCollection, Configuration configuration)
    {
        serviceCollection.AddSingleton<IBotDatabase>(provider => new BotDatabase(configuration));

        return serviceCollection;
    }
    
    public static IServiceCollection AddRepositories(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<ScenarioStateRepository>();
        serviceCollection.AddSingleton<ScenariosRepository>();
        serviceCollection.AddSingleton<UserAnswersRepository>();
        
        return serviceCollection;
    }
    
    public static IServiceCollection AddScenarios(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<ScenarioStateRepository>();
        
        serviceCollection.AddSingleton<RegisterScenario>();
        serviceCollection.AddSingleton<SettingsScenario>();
        serviceCollection.AddSingleton<GetStudentsScenario>();
        
        serviceCollection.AddSingleton<StatusScenario>();
        serviceCollection.AddSingleton<EveningStandUpScenario>();
        serviceCollection.AddSingleton<WeekendReflectionScenario>();
        
        return serviceCollection;
    }
}