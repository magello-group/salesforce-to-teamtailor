using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Magello {
    
    public static class TeamTailorAPI {

        private static readonly string? ApiToken = Environment.GetEnvironmentVariable("TEAMTAILOR_API_TOKEN");
        private static readonly string ApiHost= "api.teamtailor.com";
        private static readonly string ApiVersion = "v1";

        public async static Task<List<TeamTailorApplicationData>> GetApplications(
            DateTime since, 
            ILogger _logger) 
        {
            var applications = await Get<TeamTailorApplications>(
                Utils.CreateUrl(
                    ApiHost,
                    $"{ApiVersion}/job-applications",
                    new Dictionary<string, string>() {
                        { "filter[created-at][from]", since.ToString("yyyy-MM-dd") }
                    }),
                _logger);
            List<TeamTailorApplicationData> applicationData = new ();
            if (applications == null)
                return applicationData;
            foreach (var application in applications) {
                applicationData.AddRange(application.Data);
            }
            return applicationData;
        }

        public async static Task<TeamTailorJob?> GetJob(string link, ILogger _logger) {
            var jobs = await Get<TeamTailorJob>(link, _logger);
            if (jobs == null || jobs.Count() == 0)
                return null;
            return jobs.First();
        }

        public async static Task<HttpResponseMessage> CreateJob(TeamTailorPostJob job, ILogger _logger) {
            return await Post<TeamTailorPostJob>($"{ApiVersion}/jobs", null, job, _logger);
        }

        public async static Task<List<TeamTailorJobData>?> GetTaggedJobs(ILogger _logger) {
            var jobs = await Get<TeamTailorJobs>(
                Utils.CreateUrl(
                    ApiHost,
                    $"{ApiVersion}/jobs",
                    new Dictionary<string, string>() { {"filter[tags]", "salesforce"} }),
                _logger);
            List<TeamTailorJobData> jobData = new List<TeamTailorJobData>();
            if (jobs == null)
                return jobData;
            foreach (var job in jobs)
                jobData.AddRange(job.Data);
            LoadOpportunityRefNrFromTags(jobData);
            return jobData;
        }

        public static void LoadOpportunityRefNrFromTags(List<TeamTailorJobData> jobs) {
            var pattern = $"{Mappings.SfRefTagPrefix}(.+)";
            // Sanity checks
            foreach (var job in jobs) {
                if (job.Attributes == null || 
                    job.Attributes.Tags == null ||
                    job.Attributes.Tags.Count == 0)
                    return;
                foreach (var t in job.Attributes.Tags) {
                    var idMatch = Regex.Match(t, pattern);
                    if (idMatch.Success)
                        job.Attributes.SalesForceInternalRefId = idMatch.Groups[1].Value;
                }
            }
        }

        // Handles pagination through IPageable
        private async static Task<List<T>?> Get<T>(string url, ILogger _logger) where T : IPageable {
            _logger.LogInformation($"Calling GET {url}");
            var result = new List<T>();
            using HttpClient client = new ();
            InitClient(client);
            var stringResponse = await client.GetStringAsync(url);
            var response = JsonSerializer.Deserialize<T>(stringResponse, Utils.GetJsonSerializer());
            if (response != null)
                result.Add(response);
            if (response?.GetNextUrl() != null) {
                stringResponse = await client.GetStringAsync(response.GetNextUrl());
                response = JsonSerializer.Deserialize<T>(stringResponse, Utils.GetJsonSerializer());
                if (response != null)
                    result.Add(response);    
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
