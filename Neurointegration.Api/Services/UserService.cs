using FluentValidation;
using Neurointegration.Api.DataModels.Dto;
using Neurointegration.Api.DataModels.Models;
using Neurointegration.Api.Excpetions;
using Neurointegration.Api.Extensions;
using Neurointegration.Api.Helpers;
using Neurointegration.Api.Storages;
using Neurointegration.Api.Storages.Questions;

namespace Neurointegration.Api.Services;

public class UserService : IUserService
{
    private readonly IUsersStorage usersStorage;
    private readonly ISprintService sprintService;
    private readonly IGoogleStorage googleStorage;
    private readonly IQuestionStorage questionStorage;
    private readonly QuestionHelper questionHelper;
    private readonly IValidator<User> userValidator;

    public UserService(
        IUsersStorage usersStorage,
        ISprintService sprintService,
        IGoogleStorage googleStorage,
        IQuestionStorage questionStorage,
        QuestionHelper questionHelper,
        IValidator<User> userValidator)
    {
        this.usersStorage = usersStorage;
        this.sprintService = sprintService;
        this.googleStorage = googleStorage;
        this.questionStorage = questionStorage;
        this.questionHelper = questionHelper;
        this.userValidator = userValidator;
    }

    public async Task<User> CreateUser(CreateUser createUser)
    {
        var storedUser = await usersStorage.GetUser(createUser.UserId);
        if (storedUser != null)
            throw new DataConflictException("User с таким id уже существует");

        var user = new User(createUser);
        IsUserValid(user);
        if (createUser.SendRegularMessages)
        {
            if (createUser.SprintStartDate == null || createUser.FirstReflectionDate == null)
                throw new ValidationException("Указаны не все обязательные поля для прохождения спринта");
            var sprint = await sprintService
                .CreateSprint(user, 0, createUser.SprintStartDate.Value);
            user.Sprints.Add(sprint);
            await AddRegularQuestions(user, sprint, createUser.FirstReflectionDate.Value);
        }

        await usersStorage.SaveUser(user);

        return user;
    }

    public async Task<User> UpdateUser(UpdateUser updateUser)
    {
        var storedUser = await GetUser(updateUser.UserId);

        if (updateUser.Email != null)
            storedUser.Email = updateUser.Email;

        if (updateUser.IAmCoach != null)
            storedUser.IAmCoach = updateUser.IAmCoach.Value;

        if (updateUser.MessageEndTime != null)
            storedUser.MessageEndTime = updateUser.MessageEndTime.Value;

        if (updateUser.MessageStartTime != null)
            storedUser.MessageStartTime = updateUser.MessageStartTime.Value;

        if (updateUser.EveningStandUpTime != null)
            storedUser.EveningStandUpTime = updateUser.EveningStandUpTime.Value;

        if (storedUser.SendRegularMessages == false && updateUser.SendRegularMessages == true)
        {
            if (updateUser.ReflectionDate == null || storedUser.MessageEndTime == null ||
                storedUser.MessageStartTime == null || storedUser.EveningStandUpTime == null ||
                updateUser.SprintStartDate == null)
                throw new ArgumentException("Для добавления отправки сообщений указаны не все обязательные параметры");

            var (sprint, lastSprintNumber) = await sprintService.GetActiveSprint(updateUser.UserId);
            if (sprint == null)
                sprint = await sprintService.CreateSprint(storedUser, lastSprintNumber + 1,
                    updateUser.SprintStartDate.Value);
            await AddRegularQuestions(storedUser, sprint, updateUser.ReflectionDate.Value);
        }

        if (storedUser.SendRegularMessages == true && updateUser.SendRegularMessages is true or null)
        {
            if (updateUser.ReflectionDate != null)
            {
                var question = await questionStorage.Get(storedUser.UserId, ScenarioType.Reflection);
                var newQuestion = new Question(question.Value);
                newQuestion.Date = updateUser.ReflectionDate.Value;
                await questionStorage.UpdateQuestion(question.Value, newQuestion);
            }

            if (updateUser.EveningStandUpTime != null)
            {
                var question = await questionStorage.Get(storedUser.UserId, ScenarioType.EveningStandUp);
                var newQuestion = new Question(question.Value);
                newQuestion.Date = question.Value.Date + updateUser.EveningStandUpTime.Value;
                await questionStorage.UpdateQuestion(question.Value, newQuestion);
            }

            if (updateUser.MessageStartTime != null || updateUser.MessageEndTime != null)
            {
                var question = await questionStorage.Get(storedUser.UserId, ScenarioType.Status);
                var newQuestion = new Question(question.Value);
                newQuestion.Date = question.Value.Date + questionHelper.GetNewStatusQuestionTime(newQuestion, storedUser);
                await questionStorage.UpdateQuestion(question.Value, newQuestion);
            }
        }

        if (storedUser.SendRegularMessages == true && updateUser.SendRegularMessages == false)
        {
            await questionStorage.DeleteUserQuestions(storedUser.UserId);
        }


        IsUserValid(storedUser);
        await usersStorage.SaveUser(storedUser);

        return storedUser;
    }

    public async Task<List<User>> GetStudents(long coachId)
    {
        var studentIds = (await usersStorage.GetAccessUsers(coachId)).ToList();
        var students = new List<User>(studentIds.Count);
        foreach (var studentId in studentIds)
            students.Add(await usersStorage.GetUser(studentId));

        return students;
    }

    private async Task AddRegularQuestions(User user, Sprint sprint, DateTime firstReflectionDate)
    {
        var startMessageDay = sprint.SprintStartDate < DateTime.UtcNow.Date
            ? DateTime.UtcNow.Date
            : sprint.SprintStartDate;
        var questionEveningStandUp = new Question(
            startMessageDay + user.EveningStandUpTime.Value,
            user.UserId,
            ScenarioType.EveningStandUp,
            sprint.SprintNumber,
            0,
            0);
        var reflectionStart = firstReflectionDate;
        while (reflectionStart < DateTime.UtcNow.Date)
        {
            reflectionStart = reflectionStart.AddDays(7).Add(user.WeekReflectionTime.Value);
        }

        var questionReflection = new Question(
            reflectionStart,
            user.UserId,
            ScenarioType.Reflection,
            sprint.SprintNumber,
            0,
            0);

        var questionStatus = new Question(
            startMessageDay,
            user.UserId,
            ScenarioType.Status,
            sprint.SprintNumber,
            -1,
            1);
        questionStatus.Date += questionHelper.GetNewStatusQuestionTime(questionStatus, user);
        questionStatus.SprintReplyNumber = 0;

        // TODO: удалить созданные, если одно из сохранений не удалось
        try
        {
            await questionStorage.AddOrReplace(questionEveningStandUp);
            await questionStorage.AddOrReplace(questionReflection);
            await questionStorage.AddOrReplace(questionStatus);
        }
        catch (Exception e)
        {
            await questionStorage.DeleteUserQuestions(user.UserId);
            throw;
        }
    }

    public async Task<User> GetUser(long userId)
    {
        var user = await usersStorage.GetUser(userId);
        if (user == null)
            throw new NotFoundException($"Пользователя c id={userId} не существует");

        return user;
    }

    public async Task GrantedAccess(long ownerId, long grantedUserId)
    {
        var ownerSheets = await sprintService.GetUserGoogleSheets(ownerId);
        var grantedUser = await GetUser(grantedUserId);

        if (ownerSheets.Count == 0)
            throw new ArgumentException($"У пользователя id={ownerId} нет гугл таблиц");

        foreach (var sheetId in ownerSheets)
        {
            var permissionId = await googleStorage.GrantedAccessSheet(sheetId, grantedUser.Email);
            await usersStorage.AddAccess(grantedUser.UserId, ownerId, sheetId, permissionId);
        }
    }

    public async Task DeleteAccess(long ownerId, long grantedUserId)
    {
        var permissions = await usersStorage.GetPermissions(grantedUserId, ownerId);

        if (permissions.Count == 0)
            return;

        foreach (var permission in permissions)
            await googleStorage.DeleteAccessSheets(permission.SheetId, permission.PermissionId);

        await usersStorage.DeleteAccess(grantedUserId, ownerId);
    }

    public async Task UpdateAccess(long userId, string sheetId)
    {
        var grantedUsers = await usersStorage.GetGrantedUsers(userId);
        foreach (var grantedUserId in grantedUsers)
        {
            var grantedUser = await GetUser(grantedUserId);
            var permissionId = await googleStorage.GrantedAccessSheet(sheetId, grantedUser.Email);
            await usersStorage.AddAccess(grantedUserId, userId, sheetId, permissionId);
        }
    }

    public async Task<List<Sprint>> GetSprints(long ownerId)
    {
        var sprints = await sprintService.GetSprints(ownerId);

        return sprints;
    }


    public async Task<List<string>> GetSpreadSheets(long userId)
    {
        var sheets = await sprintService.GetUserGoogleSheets(userId);

        return sheets;
    }

    public async Task<bool> HaveAccess(long grantedUserId, long ownerId)
    {
        var userIds = await usersStorage.GetAccessUsers(grantedUserId);

        return userIds.Any(id => id == ownerId);
    }

    public async Task<List<User>> GetPublicCoachs()
    {
        return await usersStorage.GetPublicCoachs();
    }

    private void IsUserValid(User user)
    {
        var validationResult = userValidator.Validate(user);
        if (!validationResult.IsValid)
            throw new ArgumentException(string.Join(", ", validationResult.Errors.Select(x => x.ErrorMessage)));
    }
}