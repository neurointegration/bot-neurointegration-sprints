using System.Text;
using Grpc.Core.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Neurointegration.Api.DataModels.Models;
using Neurointegration.Api.DI;
using Neurointegration.Api.Google;
using Neurointegration.Api.Helpers;
using Neurointegration.Api.Services;
using Neurointegration.Api.Settings;
using Neurointegration.Api.Storages;
using Neurointegration.Api.Storages.Answers;
using Neurointegration.Api.Storages.Mapper;
using Neurointegration.Api.Storages.Questions;
using Neurointegration.Api.Storages.Sprints;

namespace BotTemplate.Client;

public static class InitializeLocalClient
{
    // public static ServiceProvider Init()
    // {
    //     sb.Append($"start settings");
    //     var service = new ServiceCollection();
    //     sb.Append($"get settings");
    //     var settings = ApiSecretSettings.FromEnvironment();
    //     sb.Append($"settings {settings}");
    //     return service
    //         .AddLogging()
    //         .AddInternalDependencies(settings)
    //         .AddSingleton<IBackendApiClient, LocalBackendApiClient>()
    //         .BuildServiceProvider();
    // }

    public static LocalBackendApiClient Init()
    {
        var settings = ApiSecretSettings.FromEnvironment();
        var googleUtils = new GoogleSheetUtils();
        var googleSheetsClient = new GoogleSheetClient(settings);
        var googleDriveClient = new GoogleDriveClient(settings);
        var googleStorage = new GoogleStorage(googleSheetsClient, googleDriveClient, googleUtils);
        var ydbClient = new YdbClient(settings);
        var sprintMapper = new SprintMapper();
        var sprintStorage = new SprintStorage(ydbClient, sprintMapper);
        var answerService = new AnswersService(googleStorage, googleUtils, sprintStorage);
        var questionMapper = new QuestionMapper();
        var questionStorage = new QuestionStorage(ydbClient, questionMapper);
        var questionHelper = new QuestionHelper();

        var sprintService = new SprintService(googleStorage, sprintStorage);
        var userStorage = new UsersStorage(ydbClient, new UserMapper());
        var userService = new UserService(userStorage, sprintService, googleStorage, questionStorage, questionHelper,
            new UserValidator());

        var questionService = new QuestionService(questionStorage, userService, sprintService, questionHelper);

        return new LocalBackendApiClient(answerService, questionService, userService);
    }
}