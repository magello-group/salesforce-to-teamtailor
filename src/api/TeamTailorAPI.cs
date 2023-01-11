using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;

namespace Magello {
    
    public static class TeamTailorAPI {

        private static readonly string ApiHost= "api.teamtailor.com";
        private static readonly string ApiVersion = "v1";

        public async static Task<HttpResponseMessage> CreateCustomFieldMappings(JsonNode values, ILogger _logger) {
            return await Post<JsonNode>(
                $"{ApiVersion}/custom-field-values",
                null,
                values,
                _logger
            );
        }

        // Returns a dictionary<string, string> with custom-field id to custom-field-value value
        // mappings for a given job
        public async static Task<Dictionary<string, string>?> GetCustomFieldValues(
            JsonNode job, 
            ILogger _logger)
        {
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

            if (values == null || values.Count == 0) {
                return null;
            }

            Dictionary<string, string> fieldValues = new Dictionary<string, string>();
            foreach (var result in values) {
                var data = result?["data"];
                if (data == null)
                    continue;
                foreach (var value in data.AsArray()) {
                    var fieldLink = value?["relationships"]?["custom-field"]?["links"]?["related"]?.GetValue<string>();
                    if (fieldLink == null)
                        continue;
                    var field = await Get(fieldLink, _logger);
                    if (field == null || field.Count == 0)
                        continue;
                    var fieldId = field[0]["data"]?["id"]?.GetValue<string>();
                    var fieldValue = value?["attributes"]?["value"]?.GetValue<string>();
                    if (fieldId != null && fieldValue != null)
                        fieldValues.Add(fieldId, fieldValue);
                }
            }

            return fieldValues;
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
                if (application == null || !(application is JsonObject data))
                    continue;
                if (!data.ContainsKey("data") || data["data"] == null)
                    continue;
                var dataArray = data["data"].AsArray();
                foreach (var obj in dataArray) {
                    var unlinkedObj = obj.Deserialize<JsonNode>();
                    applicationData.Add(unlinkedObj);
                }
            }
            return applicationData;
        }

        public async static Task<JsonNode?> GetCandidateFromApplication(
            JsonNode application,
            ILogger _logger
        ) {
            var link = application["relationships"]?["candidate"]?["links"]?["related"]?.GetValue<string>();
            if (link == null)
                return null;
            var candidates = await Get(link, _logger);
            if (candidates == null || candidates.Count() == 0)
                return null;
            return candidates.First();
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

        // Handles pagination
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
                "Token", $"token={Envs.GetEnvVar(Envs.E_TeamTailorApiToken)}"
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
