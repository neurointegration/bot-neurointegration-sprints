using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Neurointegration.Api.DataModels.Models;
using Neurointegration.Api.Services;

namespace Neurointegration.Api.Controllers;

[ApiController]
[Route("question")]
[Authorize(Policy = "ApiKey")]
public class QuestionsController : ControllerBase
{
    private readonly IQuestionService questionService;
    private readonly ILogger<QuestionsController> log;

    public QuestionsController(IQuestionService questionService, ILogger<QuestionsController> log)
    {
        this.questionService = questionService;
        this.log = log;
    }

    [HttpGet("{time}")]
    public async Task<IActionResult> GetQuestions(int time)
    {
        log.Log(LogLevel.Information, "Accept request GET /question/{time}", time);

        var questions = await questionService.Get(time);
        return Ok(questions);
    }
}