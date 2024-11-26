using Microsoft.AspNetCore.Authorization;

namespace Neurointegration.Api.Authorizations;

public class ApiKeyRequirement : IAuthorizationRequirement
{
    public HashSet<string> ApiKeys { get; }

    public ApiKeyRequirement(HashSet<string> apiKeys)
    {
        ApiKeys = apiKeys;
    }

    public static ApiKeyRequirement FromConfiguration(ConfigurationManager configurationManager)
    {
        var apiKeys = configurationManager
            .GetSection("Settings:Authorization:ApiKeys")
            .Get<HashSet<string>>();

        return new ApiKeyRequirement(apiKeys ?? new HashSet<string>());
    }
}