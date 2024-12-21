using BotTemplate.Client;
using Newtonsoft.Json;
using Yandex.Cloud.Functions;

namespace BotTemplate;

public class GetCoachListHandler : BaseFunctionHandler<IBackendApiClient>
{
    protected override async Task<string> InnerHandleRequest(string request, Context context)
    {
        var listCoach = await handleService.GetPublicCoachListAsync();
        return JsonConvert.SerializeObject(listCoach);
    }

    protected override string LogCategoryName { get; set; } = "GetCoachListHandler";
}