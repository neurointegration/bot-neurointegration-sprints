using Neurointegration.Api.DataModels.Dto;
using Neurointegration.Api.DataModels.Models;
using Neurointegration.Api.DataModels.Result;
using Neurointegration.Api.Services;

namespace BotTemplate.Client;

public class LocalBackendApiClient : IBackendApiClient
{
    private readonly IAnswersService answersService;
    private readonly IQuestionService questionService;
    private readonly IUserService userService;

    public LocalBackendApiClient(IAnswersService answersService, IQuestionService questionService,
        IUserService userService)
    {
        this.answersService = answersService;
        this.questionService = questionService;
        this.userService = userService;
    }

    public async Task SendAnswerAsync(SendAnswer sendAnswer)
    {
        var answer = await answersService.Save(sendAnswer);
        if (!answer.IsSuccess)
            throw new HttpRequestException(answer.Error.Message);
    }

    public async Task<List<Question>> GetQuestionsAsync(int timePeriod, ScenarioType? scenarioType)
    {
        var questionsActionResult = await questionService.Get(timePeriod, scenarioType);

        return questionsActionResult;
    }

    public async Task<User> CreateUserAsync(CreateUser createUser)
    {
        var result = await userService.CreateUser(createUser);

        return result;
    }

    public async Task<User> UpdateUserAsync(UpdateUser updateUser)
    {
        var result = await userService.UpdateUser(updateUser);
        if (!result.IsSuccess)
            throw new HttpRequestException(result.Error.Message);
        return result.Value;
    }

    public async Task<Result<User>> GetUser(long userId)
    {
        return await userService.GetUser(userId);
    }

    public async Task<List<Sprint>> GetUserSprintsAsync(long ownerId, long grantedUserId)
    {
        var result = await userService.GetSprints(ownerId);

        return result;
    }

    public async Task<List<Sprint>> GetUserSprintsAsync(string username)
    {
        var result = await userService.GetSprints(username);
        if (!result.IsSuccess)
            throw new HttpRequestException(result.Error.Message);
        return result.Value;
    }

    public async Task GrantedAccessToUserInfoAsync(long ownerId, long grantedUserId)
    {
        await userService.GrantedAccess(ownerId, grantedUserId);
    }

    public async Task DeleteAccessToUserInfoAsync(long ownerId, long grantedUserId)
    {
        await userService.DeleteAccess(ownerId, grantedUserId);
    }

    public async Task<List<User>> GetPublicCoachListAsync()
    {
        var result = await userService.GetPublicCoachs();

        return result;
    }

    public async Task<List<User>> GetCoachStudentsAsync(long coachId)
    {
        var result = await userService.GetStudents(coachId);
        if (!result.IsSuccess)
            throw new HttpRequestException(result.Error.Message);
        return result.Value;
    }
}