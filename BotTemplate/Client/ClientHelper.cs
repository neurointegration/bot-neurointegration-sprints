using System.Text;
using Newtonsoft.Json;

namespace BotTemplate.Client;

public static class ClientHelper
{
    public static StringContent BuildContent<T>(T obj)
    {
        var jsonObject = JsonConvert.SerializeObject(obj);
        return new StringContent(jsonObject, Encoding.UTF8, "application/json");
    }

    public static async Task<T?> ParseContent<T>(this HttpResponseMessage response)
    {
        var responseBody = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(responseBody);
    }

    public static async Task EnsureSuccess(this HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.ParseContent<ResponseExceptionDto>() ?? new ResponseExceptionDto();
            throw new HttpRequestException(error.Message);
        }
    }

    public static HttpClient WithDefaultAuthorization(this HttpClient client, string apiKey)
    {
        client.DefaultRequestHeaders.Add("Authorization", apiKey);
        return client;
    }
}