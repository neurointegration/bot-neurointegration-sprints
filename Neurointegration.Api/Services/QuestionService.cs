using Neurointegration.Api.DataModels.Models;
using Neurointegration.Api.DataModels.Result;
using Neurointegration.Api.Excpetions;
using Neurointegration.Api.Helpers;
using Neurointegration.Api.Storages.Questions;

namespace Neurointegration.Api.Services;

public class QuestionService : IQuestionService
{
    private readonly IQuestionStorage questionStorage;
    private readonly IUserService userService;
    private readonly ISprintService sprintService;
    private readonly QuestionHelper questionHelper;
    private readonly ILogger log;

    public QuestionService(
        IQuestionStorage questionStorage,
        IUserService userService,
        ISprintService sprintService,
        QuestionHelper questionHelper,
        ILogger log)
    {
        this.questionStorage = questionStorage;
        this.userService = userService;
        this.sprintService = sprintService;
        this.questionHelper = questionHelper;
        this.log = log;
    }

    public async Task<List<Question>> Get(int time, ScenarioType? scenarioType)
    {
        var questions = await questionStorage.Get(DateTime.UtcNow.AddMinutes(time), scenarioType);
        var sendUsers = new HashSet<long>();
        var resultQuestion = new List<Question>();

        foreach (var question in questions.OrderByDescending(question => question.Priority))
        {
            if (sendUsers.Contains(question.UserId))
                continue;

            switch (question.ScenarioType.IsRegularEvent())
            {
                case true:
                    var updateResult = await CreateNextQuestionWithLogs(question);
                    if (!updateResult.IsSuccess)
                        continue;
                    break;
                case false:
                    var deleteResult = await Delete(question);
                    if (!deleteResult.IsSuccess)
                        continue;
                    break;
            }

            sendUsers.Add(question.UserId);
            resultQuestion.Add(question);
        }

        return resultQuestion;
    }

    private async Task<Result> CreateNextQuestionWithLogs(Question question)
    {
        log.LogDebug($"Попытка обновить вопрос {question}");
        
        var updateResult = await CreateNextQuestion(question);
        
        if (!updateResult.IsSuccess)
            log.LogError($"Неудалось обновить вопрос {question}. Ошибка {updateResult.Error}");

        return updateResult;
    }

    private async Task<Result> CreateNextQuestion(Question question)
    {
        var updateQuestion = new Question(question);

        var activeSprint = await sprintService.GetSprint(question.UserId, question.SprintNumber);
        if (!activeSprint.IsSuccess)
            return activeSprint;
        
        updateQuestion.Date = await GetNewQuestionDateTime(question);
        updateQuestion.SprintReplyNumber += 1;
        updateQuestion.SprintNumber = GetNextQuestionSprintNumber(question, activeSprint.Value);
        if (updateQuestion.SprintNumber != question.SprintNumber)
        {
            updateQuestion.SprintReplyNumber = 0;
            var result = await CreateSprintIfNotExist(question.UserId, updateQuestion.SprintNumber,
                activeSprint.Value.SprintStartDate.AddDays(SprintConstants.SprintDaysCount));
            if (!result.IsSuccess)
                return result;
        }

        await questionStorage.UpdateQuestion(question, updateQuestion);

        return Result.Success();
    }

    private async Task<Result> Delete(Question question)
    {
        log.LogDebug($"Попытка удалить вопрос {question}");
        
        var deleteResult =  await questionStorage.Delete(question);
        
        if (!deleteResult.IsSuccess)
            log.LogError($"Неудалось удалить вопрос {question}. Ошибка {deleteResult.Error}");

        return deleteResult;
    }

    private long GetNextQuestionSprintNumber(Question question, Sprint sprint)
    {
        switch (question.ScenarioType)
        {
            case ScenarioType.EveningStandUp:
                if (question.SprintReplyNumber == SprintConstants.SprintEveningStandUpCount)
                    return question.SprintNumber + 1;
                break;
            case ScenarioType.Reflection:
                if (question.SprintReplyNumber == SprintConstants.SprintReflectionCount)
                    return question.SprintNumber + 1;
                break;
            case ScenarioType.Status:
                if (question.SprintReplyNumber == SprintConstants.SprintStatusCount)
                    return question.SprintNumber + 1;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        var sprintDay = DateOnly.FromDateTime(DateTime.UtcNow.Date).DayNumber -
                        DateOnly.FromDateTime(sprint.SprintStartDate).DayNumber;
        if (sprintDay == SprintConstants.SprintDaysCount)
            return question.SprintNumber + 1;

        return question.SprintNumber;
    }

    private async Task<DateTime> GetNewQuestionDateTime(Question question)
    {
        var user = await userService.GetUser(question.UserId);
        switch (question.ScenarioType)
        {
            case ScenarioType.EveningStandUp:
                return question.Date.AddDays(1);
            case ScenarioType.Reflection:
                return question.Date.AddDays(7);
            case ScenarioType.Status:
                return questionHelper.GetNewStatusQuestionDate(question, user);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private async Task<Result> CreateSprintIfNotExist(long userId, long sprintNumber, DateTime sprintStartDate)
    {
        var getSprint = await sprintService.GetSprint(userId, sprintNumber);
        if (getSprint.Error.Status == ErrorStatus.NotFound)
        {
            var user = await userService.GetUser(userId);
            var sprint = await sprintService.CreateSprint(user, sprintNumber, sprintStartDate);
            await userService.UpdateAccess(user.UserId, sprint.SheetId);
        }

        return getSprint;
    }
}