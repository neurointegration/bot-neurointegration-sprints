using Neurointegration.Api.DataModels.Dto;
using Neurointegration.Api.DataModels.Models;
using Neurointegration.Api.DataModels.Result;

namespace Neurointegration.Api.Services;

public interface IUserService
{
    Task<User> CreateUser(CreateUser createUser);
    Task<Result<User>> GetUser(long userId);
    Task<Result> GrantedAccess(long ownerId, long grantedUserId);
    Task<List<string>> GetSpreadSheets(long userId);
    Task<bool> HaveAccess(long grantedUserId, long ownerId);
    Task<List<User>> GetPublicCoachs();
    Task<Result<User>> UpdateUser(UpdateUser updateUser);
    Task<Result<List<User>>> GetStudents(long userId);
    Task DeleteAccess(long ownerId, long grantedUserId);
    Task<Result> UpdateAccess(long userId, string sheetId);
    Task<List<Sprint>> GetSprints(long ownerId);
    Task<Result<List<Sprint>>> GetSprints(string username);
    Task DeleteUser(long userId);
}