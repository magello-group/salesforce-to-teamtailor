using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Magello.SalesforceToTeamtailor.Api;

internal sealed class TeamTailorAPI(HttpClient client, IConfiguration configuration, ILogger<TeamTailorAPI> logger) : ITeamTailorApi
{

    private static readonly string ApiHost = "api.teamtailor.com";
    private static readonly string ApiVersion = "v1";

    private readonly HttpClient _client = client;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger _logger = logger;

    public async Task<HttpResponseMessage> CreateCustomFieldMappings(JsonNode values)
    {
        return await Post($"{ApiVersion}/custom-field-values", values);
    }

    // Returns a dictionary<string, string> with custom-field id to custom-field-value value
    // mappings for a given job
    public async Task<Dictionary<string, string>?> GetCustomFieldValues(JsonNode job)
    {
        var link = job["data"]?["relationships"]?["custom-field-values"]?["links"]?["related"]?.GetValue<string>();

        if (link == null)
        {
            return null;
        }
        var values = await Get(link);

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

                var field = await Get(fieldLink);
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

    public async Task<JsonNode?> GetCustomFields()
    {
        var fields = await Get(
            Utils.Utils.CreateUrl(
                ApiHost,
                $"{ApiVersion}/custom-fields",
                null
            ));
        return fields?[0];
    }

    public async Task<JsonArray> GetApplications(DateTime since)
    {
        var applications = await Get(
            Utils.Utils.CreateUrl(
                ApiHost,
                $"{ApiVersion}/job-applications",
                new Dictionary<string, string>() {
                    { "filter[created-at][from]", since.ToString("yyyy-MM-dd") }
                }));
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

    public async Task<JsonNode?> GetCandidateFromApplication(JsonNode application)
    {
        var link = application["relationships"]?["candidate"]?["links"]?["related"]?.GetValue<string>();
        if (link == null)
        {
            return null;
        }

        var candidates = await Get(link);
        if (candidates == null || candidates.Count == 0)
        {
            return null;
        }

        return candidates.First();
    }

    public async Task<JsonNode?> GetJobFromApplication(JsonNode application)
    {
        var link = application["relationships"]?["job"]?["links"]?["related"]?.GetValue<string>();
        if (link == null)
        {
            return null;
        }

        var jobs = await Get(link);
        if (jobs == null || jobs.Count == 0)
        {
            return null;
        }

        return jobs.First();
    }

    public async Task<HttpResponseMessage> CreateJob(JsonNode job)
    {
        return await Post($"{ApiVersion}/jobs", job);
    }

    // Handles pagination
    private async Task<List<JsonNode>?> Get(string url)
    {
        _logger.LogInformation("Calling GET {url}", url);
        var result = new List<JsonNode>();

        var stringResponse = await _client.GetStringAsync(url);
        var response = JsonSerializer.Deserialize<JsonNode>(stringResponse, Utils.Utils.GetJsonSerializer());
        if (response != null)
        {
            result.Add(response);
        }

        if (Utils.Utils.GetNextLink(response) != null)
        {
            do
            {
                stringResponse = await _client.GetStringAsync(Utils.Utils.GetNextLink(response));
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

    private async Task<HttpResponseMessage> Post<T>(string endpoint, T jsonData)
    {
        _logger.LogInformation("Calling POST {Endpoint}", endpoint);

        var json = JsonSerializer.Serialize(jsonData, Utils.Utils.GetJsonSerializer());

        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        return await _client.PostAsync(endpoint, content);
    }
}
