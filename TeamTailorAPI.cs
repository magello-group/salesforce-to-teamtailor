using System.Text;
using System.Text.Json;
using Magello;
using Microsoft.Extensions.Logging;

public static class TeamTailorAPI {

    private static string ApiToken = Environment.GetEnvironmentVariable("TEAMTAILOR_API_TOKEN");
    private static string ApiBaseUrl = "https://api.teamtailor.com/v1";

    public async static Task<HttpResponseMessage> CreateJob(TeamTailorJob job, ILogger _logger) {
        return await Post<TeamTailorJob>("/jobs", job, _logger);
    }

    private async static Task<string> Get(string endpoint) {
        var url = CreateUrl(endpoint);
        using HttpClient client = new ();
        var request = CreateRequest<string>(client, null);
        var json = await client.GetStringAsync(url);
        return json;
    }

    private async static Task<HttpResponseMessage> Post<T>(
        string endpoint, 
        object jsonData,
        ILogger _logger) {
        var url = CreateUrl(endpoint);
        _logger.LogInformation($"Calling endpoint {url}");
        using HttpClient client = new ();
        var request = CreateRequest(client, jsonData);

        var response = await client.PostAsync(url, request.Content);
        return response;
    }

    private static string CreateUrl(string endpoint) {
        if (!endpoint.StartsWith("/"))
            endpoint = $"/{endpoint}";
        return $"{ApiBaseUrl}{endpoint}";
    }

    private static HttpRequestMessage CreateRequest<T>(HttpClient client, T? jsonData) {
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Token", $"token={ApiToken}"
        );
        client.DefaultRequestHeaders.Add("X-Api-Version", "20210218");
        var request = new HttpRequestMessage();
        if (jsonData != null) {
            var json = JsonSerializer.Serialize<T>(jsonData, Utils.GetJsonSerializer());
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            request.Content.Headers.Remove("Content-Type");
            request.Content.Headers.Add("Content-Type", "application/vnd.api+json");
        }
        return request;
    }

}