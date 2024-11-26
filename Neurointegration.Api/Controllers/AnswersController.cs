using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neurointegration.Api.DataModels.Dto;
using Neurointegration.Api.Services;

namespace Neurointegration.Api.Controllers;

[ApiController]
[Route("answer")]
[Authorize(Policy = "ApiKey")]
public class AnswersController : ControllerBase
{
    private readonly IAnswersService answersService;
    private readonly ILogger<AnswersController> log;

    public AnswersController(IAnswersService answersService, ILogger<AnswersController> log)
    {
        this.answersService = answersService;
        this.log = log;
    }

    [HttpPost]
    public async Task<IActionResult> SaveAnswer(SendAnswer answer)
    {
        log.Log(LogLevel.Information, "Accept request POST /answer");

        await answersService.Save(answer);
        return Ok();
    }
}