using Microsoft.AspNetCore.Authorization;

namespace Neurointegration.Api.Authorizations;

public class ApiKeyHandler : AuthorizationHandler<ApiKeyRequirement>
{
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly ILogger<ApiKeyHandler> log;

    public ApiKeyHandler(IHttpContextAccessor httpContextAccessor, ILogger<ApiKeyHandler> log)
    {
        this.httpContextAccessor = httpContextAccessor;
        this.log = log;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ApiKeyRequirement requirement)
    {
        var apiKey = httpContextAccessor.HttpContext?.Request.Headers["X-Neuro-Apikey"].ToString() ?? "";
        log.Log(LogLevel.Information, "Accept request with Apikey={apiKey}", apiKey);

        if (!requirement.ApiKeys.Contains(apiKey))
        {
            context.Fail(new AuthorizationFailureReason(this, $"Unknown apikey: {apiKey}"));
            return;
        }

        context.Succeed(requirement);
    }
}