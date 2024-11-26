using Neurointegration.Api.DataModels.Dto;
using Neurointegration.Api.DataModels.Models;
using Neurointegration.Api.Services;

namespace BotTemplate.Client;

public class LocalBackendApiClient : IBackendApiClient
{
    private readonly IAnswersService answersService;
    private readonly IQuestionService questionService;
    private readonly IUserService userService;

    public LocalBackendApiClient(IAnswersService answersService, IQuestionService questionService, IUserService userService)
    {
        this.answersService = answersService;
        this.questionService = questionService;
        this.userService = userService;
    }

    public async Task SendAnswerAsync(SendAnswer sendAnswer)
    {
        await answersService.Save(sendAnswer);
    }

    public async Task<List<Question>> GetQuestionsAsync(int timePeriod)
    {
        var questionsActionResult = await questionService.Get(timePeriod);

        return questionsActionResult;
    }

    public async Task<User> CreateUserAsync(CreateUser createUser)
    {
        var result = await userService.CreateUser(createUser);
        
        return result;
    }

    public async Task<User?> UpdateUserAsync(UpdateUser updateUser)
    {
        var result = await userService.UpdateUser(updateUser);
        
        return result;
    }

    public async Task<User> GetUserAsync(long userId)
    {
        var result = await userService.GetUser(userId);
        
        return result;
    }

    public async Task<List<Sprint>> GetUserSprintsAsync(long ownerId, long grantedUserId)
    {
        var result = await userService.GetSprints(ownerId);

        return result;
    }

    public async Task GrantedAccessToUserInfoAsync(long ownerId, long grantedUserId)
    {
        await userService.GrantedAccess(ownerId, grantedUserId);
    }

    public async Task DeleteAccessToUserInfoAsync(long ownerId, long grantedUserId)
    {
        await userService.DeleteAccess(ownerId, grantedUserId);
    }

    public async Task<List<User>> GetPublicCoachsAsync()
    {
        var result = await userService.GetPublicCoachs();

        return result;
    }

    public async Task<List<User>> GetCoachStudentsAsync(long coachId)
    {
        var result = await userService.GetStudents(coachId);
        
        return result;
    }
}