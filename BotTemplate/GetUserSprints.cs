using System.Text;
using BotTemplate.Client;
using BotTemplate.Models;
using Newtonsoft.Json;
using Yandex.Cloud.Functions;

namespace BotTemplate;

public class GetUserSprints : BaseFunctionHandler<IBackendApiClient>
{
    protected override async Task<string> InnerHandleRequest(string request, Context context)
    {
        var httpRequest = JsonConvert.DeserializeObject<HttpRequest>(request)!;
        var decodeBody = Encoding.UTF8.GetString(Convert.FromBase64String(httpRequest.Body));
        var userSprintsRequest = JsonConvert.DeserializeObject<UserSprintsRequest>(decodeBody)!;
        var sprints = await HandleService.GetUserSprintsAsync(userSprintsRequest.Username);
        return JsonConvert.SerializeObject(new UserSprintsResponse(){Sprints = sprints});
    }

    protected override string LogCategoryName { get; set; } = "GetUserSprints";
}