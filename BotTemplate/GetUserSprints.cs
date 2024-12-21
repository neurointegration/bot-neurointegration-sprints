using BotTemplate.Client;
using BotTemplate.Models;
using Newtonsoft.Json;
using Yandex.Cloud.Functions;

namespace BotTemplate;

public class GetUserSprints : BaseFunctionHandler<IBackendApiClient>
{
    protected override async Task<string> InnerHandleRequest(string request, Context context)
    {
        var userSprintsRequest = JsonConvert.DeserializeObject<UserSprintsRequest>(request)!;
        var sprints = await handleService.GetUserSprintsAsync(userSprintsRequest.Username);
        return JsonConvert.SerializeObject(new UserSprintsResponse(){Sprints = sprints});
    }

    protected override string LogCategoryName { get; set; } = "GetUserSprints";
}