using Neurointegration.Api.DataModels.Dto;
using Neurointegration.Api.DataModels.Models;

namespace BotTemplate.Client;

public class HttpBackendApiClient : IBackendApiClient
{
    private readonly HttpClient client;
    private readonly string baseUrl;

    public HttpBackendApiClient(Configuration configuration)
    {
        baseUrl = configuration.BackendBaseUrl ?? "http://localhost:7145";
        client = new HttpClient().WithDefaultAuthorization(configuration.BackendApiKey ?? "");
    }

    public async Task SendAnswerAsync(SendAnswer sendAnswer)
    {
        var response = await client.PostAsync($"{baseUrl}/answer", ClientHelper.BuildContent(sendAnswer));
        await response.EnsureSuccess();
    }

    public async Task<List<Question>> GetQuestionsAsync(int timePeriod)
    {
        var response = await client.GetAsync($"{baseUrl}/question/{timePeriod}");
        await response.EnsureSuccess();

        return await response.ParseContent<List<Question>>() ?? new List<Question>();
    }

    public async Task<User> GetUserAsync(long userId)
    {
        var response = await client.GetAsync($"{baseUrl}/user/{userId}");
        await response.EnsureSuccess();
        
        return await response.ParseContent<User>();
    }
    
    public async Task<User> CreateUserAsync(CreateUser createUser)
    {
        var response = await client.PostAsync($"{baseUrl}/user", ClientHelper.BuildContent(createUser));
        await response.EnsureSuccess();
        
        return await response.ParseContent<User>();
    }

    public async Task<User> UpdateUserAsync(UpdateUser updateUser)
    {
        var response = await client.PatchAsync($"{baseUrl}/user", ClientHelper.BuildContent(updateUser));
        await response.EnsureSuccess();
        
        return await response.ParseContent<User>();
    }
    
    public async Task<List<Sprint>> GetUserSprintsAsync(long ownerId, long grantedUserId)
    {
        var response = await client.GetAsync($"{baseUrl}/user/{grantedUserId}/{ownerId}/sprints");
        await response.EnsureSuccess();

        return await response.ParseContent<List<Sprint>>() ?? new List<Sprint>();
    }

    // public async Task<List<string>?> GetUserSpreadSheetsAsync(long ownerId, long grantedUserId)
    // {
    //     var response = await client.GetAsync($"{baseUrl}/user/{grantedUserId}/{ownerId}/spreadsheets");
    //     await response.EnsureSuccess();
    //
    //     return await response.ParseContent<List<string>>();
    // }

    public async Task GrantedAccessToUserInfoAsync(long ownerId, long grantedUserId)
    {
        var response =
            await client.PutAsync($"{baseUrl}/user/{grantedUserId}/{ownerId}/access", new StringContent(""));
        await response.EnsureSuccess();
    }

    public async Task DeleteAccessToUserInfoAsync(long ownerId, long grantedUserId)
    {
        var response =
            await client.DeleteAsync($"{baseUrl}/user/{grantedUserId}/{ownerId}/access");
        await response.EnsureSuccess();
    }

    public async Task<List<User>> GetPublicCoachsAsync()
    {
        var response = await client.GetAsync($"{baseUrl}/user/coach");
        await response.EnsureSuccess();

        return await response.ParseContent<List<User>>() ?? new List<User>();
    }
    
    public async Task<List<User>> GetCoachStudentsAsync(long coachId)
    {
        var response = await client.GetAsync($"{baseUrl}/coach/{coachId}/students");
        await response.EnsureSuccess();

        return await response.ParseContent<List<User>>() ?? new List<User>();
    }
}