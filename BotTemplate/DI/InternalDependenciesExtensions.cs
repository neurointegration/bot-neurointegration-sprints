using BotTemplate.Client;
using BotTemplate.Models.Telegram;
using BotTemplate.Services.Telegram;
using BotTemplate.Services.YDB;
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
            .AddMessageView()
            .AddBotDb(configuration)
            .AddSingleton<QuestionService>()
            .AddSingleton<UserMessagesService>()
            .BuildServiceProvider();
    }

    public static IServiceCollection AddTgClient(this IServiceCollection serviceCollection, string token)
    {
        serviceCollection.AddSingleton<ITelegramBotClient>(new TelegramBotClient(token));

        return serviceCollection;
    }

    public static IServiceCollection AddMessageView(this IServiceCollection serviceCollection)
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
}