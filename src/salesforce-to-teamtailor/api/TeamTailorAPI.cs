using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Magello.SalesforceToTeamtailor.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Magello.SalesforceToTeamtailor.Api;


public static class TeamTailorAPI
{

    private static readonly string ApiHost = "api.teamtailor.com";
    private static readonly string ApiVersion = "v1";

    public static async Task<HttpResponseMessage> CreateCustomFieldMappings(JsonNode values, IConfiguration configuration, ILogger _logger)
    {
        return await Post<JsonNode>(
            $"{ApiVersion}/custom-field-values",
            null,
            values,
            configuration,
            _logger
        );
    }

    // Returns a dictionary<string, string> with custom-field id to custom-field-value value
    // mappings for a given job
    public static async Task<Dictionary<string, string>?> GetCustomFieldValues(
        JsonNode job,
        IConfiguration configuration,
        ILogger _logger)
    {
        var link =
            job["data"]?["relationships"]?["custom-field-values"]?["links"]?["related"]?
            .GetValue<string>();
        if (link == null)
        {
            return null;
        }
        var values = await Get(
            link,
            configuration,
            _logger
        );

        if (values == null || values.Count == 0)
        {
            return null;
        }

        Dictionary<string, string> fieldValues = [];
        foreach (var result in values)
        {
            var data = result?["data"];
            if (data == null)
            {
                continue;
            }

            foreach (var value in data.AsArray())
            {
                var fieldLink = value?["relationships"]?["custom-field"]?["links"]?["related"]?.GetValue<string>();
                if (fieldLink == null)
                {
                    continue;
                }

                var field = await Get(fieldLink, configuration, _logger);
                if (field == null || field.Count == 0)
                {
                    continue;
                }

                var fieldId = field[0]["data"]?["id"]?.GetValue<string>();
                var fieldValue = value?["attributes"]?["value"]?.GetValue<string>();
                if (fieldId != null && fieldValue != null)
                {
                    fieldValues.Add(fieldId, fieldValue);
                }
            }
        }

        return fieldValues;
    }

    public static async Task<JsonNode?> GetCustomFields(IConfiguration configuration, ILogger _logger)
    {
        var fields = await Get(
            Utils.Utils.CreateUrl(
                ApiHost,
                $"{ApiVersion}/custom-fields",
                null
            ),
            configuration,
            _logger);
        return fields?[0];
    }

    public static async Task<JsonArray> GetApplications(
        DateTime since,
        IConfiguration configuration,
        ILogger _logger)
    {
        var applications = await Get(
            Utils.Utils.CreateUrl(
                ApiHost,
                $"{ApiVersion}/job-applications",
                new Dictionary<string, string>() {
                    { "filter[created-at][from]", since.ToString("yyyy-MM-dd") }
                }),
            configuration,
            _logger);
        JsonArray applicationData = [];
        if (applications == null)
        {
            return applicationData;
        }

        foreach (var application in applications)
        {
            if (application == null || application is not JsonObject data)
            {
                continue;
            }

            if (!data.ContainsKey("data") || data["data"] == null)
            {
                continue;
            }

            var dataArray = data["data"]!.AsArray();
            foreach (var obj in dataArray)
            {
                var unlinkedObj = obj.Deserialize<JsonNode>();
                applicationData.Add(unlinkedObj);
            }
        }
        return applicationData;
    }

    public static async Task<JsonNode?> GetCandidateFromApplication(
        JsonNode application,
        IConfiguration configuration,
        ILogger _logger
    )
    {
        var link = application["relationships"]?["candidate"]?["links"]?["related"]?.GetValue<string>();
        if (link == null)
        {
            return null;
        }

        var candidates = await Get(link, configuration, _logger);
        if (candidates == null || candidates.Count == 0)
        {
            return null;
        }

        return candidates.First();
    }

    public static async Task<JsonNode?> GetJobFromApplication(JsonNode application, IConfiguration configuration, ILogger _logger)
    {
        var link = application["relationships"]?["job"]?["links"]?["related"]?.GetValue<string>();
        if (link == null)
        {
            return null;
        }

        var jobs = await Get(link, configuration, _logger);
        if (jobs == null || jobs.Count == 0)
        {
            return null;
        }

        return jobs.First();
    }

    public static async Task<HttpResponseMessage> CreateJob(JsonNode job, IConfiguration configuration, ILogger _logger)
    {
        return await Post($"{ApiVersion}/jobs", null, job, configuration, _logger);
    }

    // Handles pagination
    private static async Task<List<JsonNode>?> Get(string url, IConfiguration configuration, ILogger _logger)
    {
        _logger.LogInformation("Calling GET {url}", url);
        var result = new List<JsonNode>();
        using HttpClient client = new();
        InitClient(client, configuration);
        var stringResponse = await client.GetStringAsync(url);
        var response = JsonSerializer.Deserialize<JsonNode>(stringResponse, Utils.Utils.GetJsonSerializer());
        if (response != null)
        {
            result.Add(response);
        }

        if (Utils.Utils.GetNextLink(response) != null)
        {
            do
            {
                stringResponse = await client.GetStringAsync(Utils.Utils.GetNextLink(response));
                //_logger.LogInformation($"Page response: {stringResponse}");
                response = JsonSerializer.Deserialize<JsonNode>(stringResponse, Utils.Utils.GetJsonSerializer());
                if (response != null)
                {
                    result.Add(response);
                }
            } while (Utils.Utils.GetNextLink(response) != null);
        }
        return result;
    }

    private static async Task<HttpResponseMessage> Post<T>(
        string endpoint,
        Dictionary<string, string>? query,
        T jsonData,
        IConfiguration configuration,
        ILogger _logger)
    {
        var url = Utils.Utils.CreateUrl(ApiHost, endpoint, query);
        _logger.LogInformation($"Calling POST {url}");
        using HttpClient client = new();
        InitClient(client, configuration);
        var request = CreateJsonDataRequest<T>(jsonData);
        return await client.PostAsync(url, request.Content);
    }

    private static void InitClient(HttpClient client, IConfiguration configuration)
    {
        var teamTailorApiToken = configuration.GetValue<string>(Envs.E_TeamTailorApiToken) ??
            throw new InvalidOperationException($"{Envs.E_TeamTailorApiToken} not set in configuration");

        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Add("Accept", "application/vnd.api+json");
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Token", $"token={teamTailorApiToken}"
        );
        client.DefaultRequestHeaders.Add("X-Api-Version", "20210218");
    }

    private static HttpRequestMessage CreateJsonDataRequest<T>(T? jsonData)
    {
        var request = new HttpRequestMessage();
        if (jsonData != null)
        {
            var json = JsonSerializer.Serialize<T>(jsonData, Utils.Utils.GetJsonSerializer());
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            request.Content.Headers.Remove("Content-Type");
            request.Content.Headers.Add("Content-Type", "application/vnd.api+json");
        }
        return request;
    }

}
