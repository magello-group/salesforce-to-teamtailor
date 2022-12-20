using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Magello {
    
    public static class TeamTailorAPI {

        private static readonly string ApiToken = Environment.GetEnvironmentVariable("TEAMTAILOR_API_TOKEN");
        private static readonly string ApiHost= "api.teamtailor.com";
        private static readonly string ApiVersion = "v1";

        public async static Task<HttpResponseMessage> CreateJob(TeamTailorJob job, ILogger _logger) {
            return await Post<TeamTailorJob>($"{ApiVersion}/jobs", null, job, _logger);
        }

        public async static Task<List<TeamTailorJobData>?> GetTaggedJobs(ILogger _logger) {
            var jobs = await Get<TeamTailorJobs>(
                Utils.CreateUrl(
                    ApiHost,
                    $"{ApiVersion}/jobs",
                    new Dictionary<string, string>() { {"filter[tags]", "salesforce"} }),
                _logger);
            if (jobs == null)
                return null;
            List<TeamTailorJobData> jobData = new List<TeamTailorJobData>();
            jobData.AddRange(jobs.Data);
            // Handle pagination links
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
            return JsonSerializer.Deserialize<T>(stringResponse, Utils.GetJsonSerializer());
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
