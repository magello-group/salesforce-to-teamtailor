using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Magello {
    
    public static class TeamTailorAPI {

        private static readonly string? ApiToken = Environment.GetEnvironmentVariable("TEAMTAILOR_API_TOKEN");
        private static readonly string ApiHost= "api.teamtailor.com";
        private static readonly string ApiVersion = "v1";

        public async static Task<HttpResponseMessage> CreateCustomFieldMappings(JsonNode values, ILogger _logger) {
            return await Post<JsonNode>(
                Utils.CreateUrl(
                    ApiHost,
                    $"{ApiVersion}/custom-field-values",
                    null
                ),
                null,
                values,
                _logger
            );
        }

        public async static Task<JsonNode?> GetCustomFieldValues(JsonNode job, ILogger _logger) {
            var link = 
                job["data"]?["relationships"]?["custom-field-values"]?["links"]?["related"]?
                .GetValue<string>();
            if (link == null) {
                return null;
            }
            var values = await Get(
                link,
                _logger
            );
            return values == null ? null : values[0];
        }

        public async static Task<JsonNode?> GetCustomFields(ILogger _logger) {
            var fields = await Get(
                Utils.CreateUrl(
                    ApiHost,
                    $"{ApiVersion}/custom-fields",
                    null
                ),
                _logger);
            return fields == null ? null : fields[0];
        }

        public async static Task<JsonArray> GetApplications(
            DateTime since, 
            ILogger _logger) 
        {
            var applications = await Get(
                Utils.CreateUrl(
                    ApiHost,
                    $"{ApiVersion}/job-applications",
                    new Dictionary<string, string>() {
                        { "filter[created-at][from]", since.ToString("yyyy-MM-dd") }
                    }),
                _logger);
            JsonArray applicationData = new ();
            if (applications == null)
                return applicationData;
            foreach (var application in applications) {
                if (!((JsonObject)application).ContainsKey("data"))
                    continue;
                foreach (var obj in (JsonArray)application["data"]) {
                    var unlinkedObj = obj.Deserialize<JsonNode>();
                    applicationData.Add(unlinkedObj);
                }
            }
            return applicationData;
        }

        public async static Task<JsonNode?> GetJobFromApplication(JsonNode application, ILogger _logger) {
            var link = application["relationships"]?["job"]?["links"]?["related"]?.GetValue<string>();
            if (link == null)
                return null;
            var jobs = await Get(link, _logger);
            if (jobs == null || jobs.Count() == 0)
                return null;
            return jobs.First();
        }

        public async static Task<HttpResponseMessage> CreateJob(JsonNode job, ILogger _logger) {
            return await Post<JsonNode>($"{ApiVersion}/jobs", null, job, _logger);
        }

        // Returns the custom field value id for the created value
        /*public async static Task<string> CreateCustomFieldValue(
            string customFieldId,
            string value,
            ILogger _logger
        ) {
            var response = await Post<
        }*/

        /*public static void LoadDataFromTags(List<JsonNode> jobs) {
            var refPattern = $"{Mappings.SfRefTagPrefix}(.+)";
            var idPattern = $"{Mappings.SfIdTagPrefix}(.+)";
            // Sanity checks
            foreach (var job in jobs) {
                if (job.Attributes == null || 
                    job.Attributes.Tags == null ||
                    job.Attributes.Tags.Count == 0)
                    return;
                foreach (var t in job.Attributes.Tags) {
                    var refMatch = Regex.Match(t, refPattern);
                    var idMatch = Regex.Match(t, idPattern);
                    if (refMatch.Success)
                        job.Attributes.SalesForceInternalRefId = refMatch.Groups[1].Value;
                    if (idMatch.Success)
                        job.Attributes.SalesForceOpportunityId = idMatch.Groups[1].Value;
                }
            }
        }*/

        // Handles pagination through IPageable
        private async static Task<List<JsonNode>?> Get(string url, ILogger _logger) {
            _logger.LogInformation($"Calling GET {url}");
            var result = new List<JsonNode>();
            using HttpClient client = new ();
            InitClient(client);
            var stringResponse = await client.GetStringAsync(url);
            var response = JsonSerializer.Deserialize<JsonNode>(stringResponse, Utils.GetJsonSerializer());
            if (response != null)
                result.Add(response);
            if (Utils.GetNextLink(response) != null) {
                do {
                    stringResponse = await client.GetStringAsync(Utils.GetNextLink(response));
                    //_logger.LogInformation($"Page response: {stringResponse}");
                    response = JsonSerializer.Deserialize<JsonNode>(stringResponse, Utils.GetJsonSerializer());
                    if (response != null)
                        result.Add(response);
                } while (Utils.GetNextLink(response) != null);
            }
            return result;
        }

        private async static Task<HttpResponseMessage> Post<T>(
            string endpoint,
            Dictionary<string, string>? query,
            T jsonData,
            ILogger _logger) 
        {
            var url = Utils.CreateUrl(ApiHost, endpoint, query);
            _logger.LogInformation($"Calling POST {url}");
            using HttpClient client = new ();
            InitClient(client);
            var request = CreateJsonDataRequest<T>(jsonData);
            return await client.PostAsync(url, request.Content);
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

}
