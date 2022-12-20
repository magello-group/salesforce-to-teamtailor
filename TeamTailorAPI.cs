using System.Text;
using System.Text.Json;
using Magello;
using Microsoft.Extensions.Logging;

public static class TeamTailorAPI {

    private static string ApiToken = Environment.GetEnvironmentVariable("TEAMTAILOR_API_TOKEN");
    private static string ApiBaseUrl = "api.teamtailor.com";
    private static string ApiVersion = "v1";

    public async static Task<HttpResponseMessage> CreateJob(TeamTailorJob job, ILogger _logger) {
        return await Post<TeamTailorJob>("/jobs", null, job, _logger);
    }

    // Handles pagination by looking for the "salesforce" tags of opportunities
    public async static Task<List<TeamTailorJobData>?> GetTaggedJobs(ILogger _logger) {
        var jobs = await Get<TeamTailorJobs>(
            CreateUrl(
                "/jobs",
                //new Dictionary<string, string>() { {"filter[tags]", "salesforce"} }),
                null),
            _logger);
        if (jobs == null)
            return null;
        List<TeamTailorJobData> jobData = new List<TeamTailorJobData>();
        jobData.AddRange(jobs.Data);
        while (!string.IsNullOrEmpty(jobs?.Links?.Next)) {
            jobs = await Get<TeamTailorJobs>(jobs.Links.Next, _logger);
            if (jobs == null)
                break;
            jobData.AddRange(jobs.Data);
        }
        return jobData;
    }

    private async static Task<T?> Get<T>(string url, ILogger _logger) {
        _logger.LogInformation($"Calling GET {url}");
        using HttpClient client = new ();
        InitClient(client);
        var stringResponse = await client.GetStringAsync(url);
        var json = JsonSerializer.Deserialize<T>(stringResponse, Utils.GetJsonSerializer());
        return json;
    }

    private async static Task<HttpResponseMessage> Post<T>(
        string endpoint,
        Dictionary<string, string>? query,
        T jsonData,
        ILogger _logger) {
        var url = CreateUrl(endpoint, query);
        _logger.LogInformation($"Calling POST {url}");
        using HttpClient client = new ();
        InitClient(client);
        var request = CreateJsonDataRequest<T>(jsonData);
        var response = await client.PostAsync(url, request.Content);
        return response;
    }

    private static string CreateUrl(string endpoint, Dictionary<string, string>? query) {
        var uri = new UriBuilder();
        uri.Scheme = "https";
        uri.Host = ApiBaseUrl;
        if (!endpoint.StartsWith("/"))
            endpoint = $"/{endpoint}";
        // Add API version
        uri.Path = $"{ApiVersion}{endpoint}";
        if (query != null) {
            uri.Query = string.Join(
                "&",
                query.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={kvp.Value}")
            );
        }
        return uri.Uri.AbsoluteUri;
    }

    private static void InitClient(HttpClient client) {
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Add("Accept", "application/vnd.api+json");
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Token", $"token={ApiToken}"
        );
        client.DefaultRequestHeaders.Add("X-Api-Version", "20210218");
    }
 
    private static HttpRequestMessage CreateJsonDataRequest<T>(T? jsonData) {
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