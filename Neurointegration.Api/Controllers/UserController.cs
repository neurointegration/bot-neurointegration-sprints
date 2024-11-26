using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neurointegration.Api.DataModels.Dto;
using Neurointegration.Api.Services;

namespace Neurointegration.Api.Controllers;

[ApiController]
[Route("user")]
[Authorize(Policy = "ApiKey")]
public class UserController : ControllerBase
{
    private readonly IUserService userService;
    private readonly ILogger log;

    public UserController(IUserService userService, ILogger<UserController> log)
    {
        this.userService = userService;
        this.log = log;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUser createUser)
    {
        log.Log(LogLevel.Information, "Accept request POST /user");
        
        var user = await userService.CreateUser(createUser);
        return Ok(user);
    }
    
    [HttpPatch]
    public async Task<IActionResult> UpdateUser(UpdateUser updateUser)
    {
        log.Log(LogLevel.Information, "Accept request PATCH /user");
        
        var user = await userService.UpdateUser(updateUser);
        return Ok(user);
    }
    
    [HttpGet("/user/{userId:long}")]
    public async Task<IActionResult> GetUser(long userId)
    {
        log.Log(LogLevel.Information, "Accept request GET /user/{userId}", userId);
        
        return Ok(await userService.GetUser(userId));
    }

    [HttpGet("/user/coach")]
    public async Task<IActionResult> GetPublicCoachs()
    {
        log.Log(LogLevel.Information, "Accept request GET /user/coach");
        
        return Ok(await userService.GetPublicCoachs());
    }
    
    [HttpGet("/coach/{userId:long}/students")]
    public async Task<IActionResult> GetCoachStudents(long userId)
    {
        log.Log(LogLevel.Information, "Accept request GET /coach/{userId}/students", userId);
        
        return Ok(await userService.GetStudents(userId));
    }


    [HttpPut("/user/{grantedUserId:long}/{ownerId:long}/access")]
    public async Task<IActionResult> GrantedAccessToUserInfo(long ownerId, long grantedUserId)
    {
        log.Log(LogLevel.Information, "Accept request PUT /user/{grantedUserId}/{ownerId}/access", grantedUserId, ownerId);
        
        await userService.GrantedAccess(ownerId, grantedUserId);
        return Ok();
    }
    
    [HttpDelete("/user/{grantedUserId:long}/{ownerId:long}/access")]
    public async Task<IActionResult> DeleteAccessToUserInfo(long ownerId, long grantedUserId)
    {
        log.Log(LogLevel.Information, "Accept request DELETE /user/{grantedUserId}/{ownerId}/access", grantedUserId, ownerId);
        
        await userService.DeleteAccess(ownerId, grantedUserId);
        return Ok();
    }

    [HttpGet("/user/{grantedUserId:long}/{ownerId:long}/sprints")]
    public async Task<IActionResult> GetSprints(long ownerId, long grantedUserId)
    {
        log.Log(LogLevel.Information, "Accept request GET /user/{grantedUserId}/{ownerId}/spreadsheets", grantedUserId, ownerId);
        
        if (grantedUserId != ownerId && !await userService.HaveAccess(grantedUserId, ownerId))
            return StatusCode(403);
        
        var sprints = await userService.GetSprints(ownerId);
        return Ok(sprints);
    }
}