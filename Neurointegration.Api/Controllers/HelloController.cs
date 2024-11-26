using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Neurointegration.Api.Controllers;

[ApiController]
[Route("")]
public class HelloController : ControllerBase
{
    private readonly ILogger<HelloController> log;

    public HelloController(ILogger<HelloController> log)
    {
        this.log = log;
    }

    [HttpGet()]
    public async Task<IActionResult> SaveAnswer()
    {
        log.Log(LogLevel.Information, "Accept hello request");
        return Ok("Hello! 2");
    }
}