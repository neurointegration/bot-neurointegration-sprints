using BotTemplate.Client;
using BotTemplate.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Yandex.Cloud.Functions;

namespace BotTemplate;

public class GetUserSprints : BaseFunctionHandler<IBackendApiClient>
{
    protected override async Task<string> InnerHandleRequest(string request, Context context)
    {
        var httpRequest = JsonConvert.DeserializeObject<HttpRequest>(request)!;
        var userSprintsRequest = JsonConvert.DeserializeObject<UserSprintsRequest>(httpRequest.Body)!;
        var sprints = await HandleService.GetUserSprintsAsync(userSprintsRequest.Username);
        return JsonConvert.SerializeObject(new UserSprintsResponse(){Sprints = sprints});
    }

    protected override string LogCategoryName { get; set; } = "GetUserSprints";
}