using Neurointegration.Api.DataModels.Models;
using Neurointegration.Api.Extensions;
using Neurointegration.Api.Helpers;
using Neurointegration.Api.Storages.Questions;
using Neurointegration.Api.Storages.Sprints;

namespace Neurointegration.Api.Services;

public class QuestionService : IQuestionService
{
    private readonly IQuestionStorage questionStorage;
    private readonly IUserService userService;
    private readonly ISprintService sprintService;
    private readonly QuestionHelper questionHelper;

    public QuestionService(
        IQuestionStorage questionStorage,
        IUserService userService,
        ISprintService sprintService,
        QuestionHelper questionHelper)
    {
        this.questionStorage = questionStorage;
        this.userService = userService;
        this.sprintService = sprintService;
        this.questionHelper = questionHelper;
    }

    public async Task CreateQuestion(Question question)
    {
        await questionStorage.AddOrReplace(question);
    }

    public async Task<List<Question>> Get(int time)
    {
        var questions = await questionStorage.Get(DateTime.UtcNow.AddMinutes(time));
        var sendUsers = new HashSet<long>();
        var resultQuestion = new List<Question>();

        foreach (var question in questions.OrderByDescending(question => question.Priority))
        {
            if (!sendUsers.Add(question.UserId))
                continue;

            switch (question.ScenarioType.IsRegularEvent())
            {
                case true:
                    await UpdateQuestion(question);
                    break;
                case false:
                    await Delete(question);
                    break;
            }

            resultQuestion.Add(question);
        }

        return resultQuestion;
    }

    private async Task UpdateQuestion(Question question)
    {
        var updateQuestion = new Question(question);

        var activeSprint = await sprintService.GetSprint(question.UserId, question.SprintNumber);
        updateQuestion.Date = await GetNewQuestionDateTime(question);
        updateQuestion.SprintReplyNumber += 1;
        updateQuestion.SprintNumber = GetNextQuestionSprintNumber(question, activeSprint);
        if (updateQuestion.SprintNumber != question.SprintNumber)
        {
            updateQuestion.SprintReplyNumber = 0;
            var newSprint = await sprintService.GetSprint(question.UserId, updateQuestion.SprintNumber);
            if (newSprint == null)
            {
                var user = await userService.GetUser(question.UserId);
                var sprint = await sprintService.CreateSprint(user, updateQuestion.SprintNumber,
                    activeSprint.SprintStartDate.AddDays(28));
                await userService.UpdateAccess(user.UserId, sprint.SheetId);
            }
        }

        await questionStorage.UpdateQuestion(question, updateQuestion);
    }

    private async Task Delete(Question question)
    {
        await questionStorage.Delete(question);
    }

    private long GetNextQuestionSprintNumber(Question question, Sprint sprint)
    {
        switch (question.ScenarioType)
        {
            case ScenarioType.EveningStandUp:
                if (question.SprintReplyNumber == 28)
                    return question.SprintNumber + 1;
                break;
            case ScenarioType.Reflection:
                if (question.SprintReplyNumber == 4)
                    return question.SprintNumber + 1;
                break;
            case ScenarioType.Status:
                if (question.SprintReplyNumber == 83)
                    return question.SprintNumber + 1;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        var sprintDay = DateOnly.FromDateTime(DateTime.UtcNow.Date).DayNumber -
                        DateOnly.FromDateTime(sprint.SprintStartDate).DayNumber;
        if (sprintDay == 28)
            return question.SprintNumber + 1;

        return question.SprintNumber;
    }

    private async Task<DateTime> GetNewQuestionDateTime(Question? question)
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
}