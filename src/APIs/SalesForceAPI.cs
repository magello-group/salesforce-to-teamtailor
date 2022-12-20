using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

namespace  Magello
{
    
    /*
    * To get an access token for Salesforce, use this guide:
    * https://developer.salesforce.com/docs/atlas.en-us.api_rest.meta/api_rest/quickstart_oauth.htm
    */
    public static class SalesForceApi {

        private static readonly string ApiHost = "r2m.my.salesforce.com";
        private static readonly string? ApiToken = Environment.GetEnvironmentVariable("SALESFORCE_API_TOKEN");

        /*
        * Opportunity field reference:
        * https://developer.salesforce.com/docs/atlas.en-us.240.0.object_reference.meta/object_reference/sforce_api_objects_opportunity.htm
        */
        public async static Task<HttpResponseMessage> UpdateOpportunity(SalesForceJob job, ILogger _logger) {
            return await Patch<SalesForceJob>($"Opportunity/{job.Id}", null, job, _logger);
        }

        private async static Task<HttpResponseMessage> Patch<T>(
            string endpoint,
            Dictionary<string, string>? query,
            T jsonData,
            ILogger _logger) 
        {
            var url = Utils.CreateUrl(ApiHost, endpoint, query);
            _logger.LogInformation($"Calling PATCH {url}");
            using HttpClient client = new ();
            InitClient(client);
            var request = CreateJsonDataRequest<T>(jsonData);
            return await client.PatchAsync(url, request.Content);
        }

        private static HttpRequestMessage CreateJsonDataRequest<T>(T? jsonData) {
            var request = new HttpRequestMessage();
            if (jsonData != null) {
                var json = JsonSerializer.Serialize<T>(jsonData, Utils.GetJsonSerializer());
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                request.Content.Headers.Remove("Content-Type");
                request.Content.Headers.Add("Content-Type", "application/json");
            }
            return request;
        }

        private static void InitClient(HttpClient client) {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Token", $"Bearer {ApiToken}"
            );
        }
    }

}