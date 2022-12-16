using System.Text;
using System.Text.Json;
using System;

public static class TeamTailorAPI {

    private static string ApiToken = Environment.GetEnvironmentVariable("TEAM_TAILOR_API_TOKEN");
    private static string ApiBaseUrl = "https://api.teamtailor.com/v1";

    private async static Task<string> Get(string endpoint) {
        var url = CreateUrl(endpoint);
        using HttpClient client = new ();
        ConfigureHttpClient(client);
        var json = await client.GetStringAsync(url);
        return json;
    }

    public async static Task<HttpResponseMessage> Post(string endpoint, object jsonData) {
        var url = CreateUrl(endpoint);
        using HttpClient client = new ();
        ConfigureHttpClient(client);

        var json = JsonSerializer.Serialize(jsonData);
        var data = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(url, data);
        return response;
    }

    private static string CreateUrl(string endpoint) {
        if (!endpoint.StartsWith("/"))
            endpoint = $"/{endpoint}";
        return $"{ApiBaseUrl}{endpoint}";
    }

    private static void ConfigureHttpClient(HttpClient client) {
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/vnd.api+json")
        );
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            $"Token token={ApiToken}"
        );
        client.DefaultRequestHeaders.Add("X-Api-Version", "20210218");
    }

}