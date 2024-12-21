using Common.Ydb;
using FluentValidation;
using Neurointegration.Api.DataModels.Models;
using Neurointegration.Api.Google;
using Neurointegration.Api.Helpers;
using Neurointegration.Api.Services;
using Neurointegration.Api.Services.Decorators;
using Neurointegration.Api.Settings;
using Neurointegration.Api.Storages;
using Neurointegration.Api.Storages.Answers;
using Neurointegration.Api.Storages.Mapper;
using Neurointegration.Api.Storages.Questions;
using Neurointegration.Api.Storages.Sprints;

namespace Neurointegration.Api.DI;

public static class InternalDependencies
{
    public static IServiceCollection AddInternalDependencies(this IServiceCollection service,
        ApiSecretSettings secretSettings)
    {
        service.AddTransient(provider => new YdbClient(secretSettings.YdbSecretSettings, provider.GetRequiredService<ILogger>()));
        service.AddTransient(_ => new GoogleSheetClient(secretSettings));
        service.AddTransient(_ => new GoogleDriveClient(secretSettings));

        service.AddDb();

        service.AddTransient<UserMapper>();
        service.AddTransient<QuestionMapper>();
        service.AddTransient<SprintMapper>();
        service.AddTransient<GoogleSheetUtils>();
        service.AddTransient<QuestionHelper>();
        service.AddTransient<IValidator<User>, UserValidator>();

        service.AddTransient<IUsersStorage, UsersStorage>();
        service.AddTransient<IQuestionStorage, QuestionStorage>();
        service.AddTransient<ISprintStorage, SprintStorage>();
        service.AddTransient<IGoogleStorage, GoogleStorage>();
        service.AddTransient<IAnswerStorage, AnswerStorage>();

        service.AddTransient<ISprintService, SprintService>();
        service.AddTransient<IUserService, UserService>();
        service.AddTransient<IQuestionService, QuestionService>();
        service.AddTransient<IAnswersService, AnswersService>();

        service.Decorate<IQuestionService, QuestionServiceDecorator>();
        service.Decorate<IAnswersService, AnswerServiceDecorator>();

        return service;
    }
}