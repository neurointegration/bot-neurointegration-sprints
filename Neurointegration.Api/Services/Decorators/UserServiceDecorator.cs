using Neurointegration.Api.DataModels.Dto;
using Neurointegration.Api.DataModels.Models;
using Neurointegration.Api.DataModels.Result;

namespace Neurointegration.Api.Services.Decorators;

public class UserServiceDecorator : IUserService
{
    private readonly IUserService userService;
    private readonly ILogger logger;
    private readonly Dictionary<long, User> userCache = new Dictionary<long, User>();

    public UserServiceDecorator(IUserService userService, ILogger logger)
    {
        this.userService = userService;
        this.logger = logger;
    }

    public async Task<User> CreateUser(CreateUser createUser)
    {
        logger.LogInformation($"Создаем пользователя {createUser}");
        var result = await userService.CreateUser(createUser);
        userCache[result.UserId] = result;
        logger.LogInformation($"Создали пользователя {result}");
        return result;
    }

    public async Task<Result<User>> GetUser(long userId)
    {
        logger.LogInformation($"Получаем пользователя {userId}");
        if (userCache.TryGetValue(userId, out var user))
        {
            logger.LogInformation("Нашли пользователя в кэше");
            return await Task.FromResult(Result<User>.Success(user));
        }

        var result = await userService.GetUser(userId);
        if (result.Value != null)
        {
            userCache[result.Value.UserId] = result.Value;
        }
        logger.LogInformation($"Нашли пользователя в хранилище {result}");

        return result;
    }

    public async Task<Result> GrantedAccess(long ownerId, long grantedUserId)
    {
        return await userService.GrantedAccess(ownerId, grantedUserId);
    }

    public async Task<List<string>> GetSpreadSheets(long userId)
    {
        return await userService.GetSpreadSheets(userId);
    }

    public async Task<bool> HaveAccess(long grantedUserId, long ownerId)
    {
        return await userService.HaveAccess(grantedUserId, ownerId);
    }

    public async Task<List<User>> GetPublicCoachs()
    {
        return await userService.GetPublicCoachs();
    }

    public async Task<Result<User>> UpdateUser(UpdateUser updateUser)
    {
        logger.LogInformation($"Обновляем пользователя: {updateUser}");
        var result = await userService.UpdateUser(updateUser);
        userCache[result.Value.UserId] = result.Value;
        logger.LogInformation($"Обновили пользователя {result}");
        return result;
    }

    public async Task<Result<List<User>>> GetStudents(long userId)
    {
        return await userService.GetStudents(userId);
    }

    public async Task DeleteAccess(long ownerId, long grantedUserId)
    {
        await userService.DeleteAccess(ownerId, grantedUserId);
    }

    public async Task<Result> UpdateAccess(long userId, string sheetId)
    {
        return await userService.UpdateAccess(userId, sheetId);
    }

    public async Task<List<Sprint>> GetSprints(long ownerId)
    {
        return await userService.GetSprints(ownerId);
    }

    public async Task<Result<List<Sprint>>> GetSprints(string username)
    {
        return await userService.GetSprints(username);
    }

    public async Task DeleteUser(long userId)
    {
        await userService.DeleteUser(userId);
        userCache.Remove(userId);
    }
}