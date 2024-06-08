using BotTemplate.Models.ClientDto;

namespace BotTemplate.Client;

public class BackendApiClient : IBackendApiClient
{
    private readonly HttpClient client;
    private readonly string baseUrl;

    public BackendApiClient(Configuration configuration)
    {
        baseUrl = configuration.BackendBaseUrl ?? "http://localhost";
        client = new HttpClient().WithDefaultAuthorization(configuration.BackendApiKey ?? "");
    }

    public async Task SendAnswerAsync(SendAnswer sendAnswer)
    {
        var response = await client.PostAsync($"{baseUrl}/answer", ClientHelper.BuildContent(sendAnswer));
        await response.EnsureSuccess();
    }

    public async Task<List<Question>?> GetQuestionsAsync(int timePeriod)
    {
        var response = await client.GetAsync($"{baseUrl}/question/{timePeriod}");
        await response.EnsureSuccess();

        return await response.ParseContent<List<Question>>();
    }

    public async Task<ApiUser?> CreateUserAsync(CreateUser createUser)
    {
        var response = await client.PostAsync($"{baseUrl}/user", ClientHelper.BuildContent(createUser));
        await response.EnsureSuccess();
        
        return await response.ParseContent<ApiUser>();
    }

    public async Task<ApiUser?> UpdateUserAsync(UpdateUser updateUser)
    {
        var response = await client.PatchAsync($"{baseUrl}/user", ClientHelper.BuildContent(updateUser));
        await response.EnsureSuccess();
        
        return await response.ParseContent<ApiUser>();
    }

    public async Task<List<string>?> GetUserSpreadSheetsAsync(long ownerId, long grantedUserId)
    {
        var response = await client.GetAsync($"{baseUrl}/user/{grantedUserId}/{ownerId}/spreadsheets");
        await response.EnsureSuccess();

        return await response.ParseContent<List<string>>();
    }

    public async Task GrantedAccessToUserInfoAsync(long ownerId, long grantedUserId)
    {
        //затестить
        var response =
            await client.PutAsync($"{baseUrl}/user/{ownerId}/access", ClientHelper.BuildContent(grantedUserId));
        await response.EnsureSuccess();
    }


    public async Task<List<ApiUser>?> GetPublicCoachsAsync()
    {
        var response = await client.GetAsync($"{baseUrl}/user/coach");
        await response.EnsureSuccess();

        return await response.ParseContent<List<ApiUser>>();
    }
}