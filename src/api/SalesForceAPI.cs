using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

namespace  Magello
{
    
    /*
    * Salesforce asnd oauth:
    * https://developer.salesforce.com/docs/atlas.en-us.api_rest.meta/api_rest/quickstart_oauth.htm
    * https://developer.salesforce.com/docs/atlas.en-us.api_rest.meta/api_rest/intro_oauth_and_connected_apps.htm
    */
    public static class SalesForceApi {

        private static readonly string ApiHost = Environment.GetEnvironmentVariable("SALESFORCE_API_HOST") ?? "";
        private static readonly string ApiTokenEndpoint = Environment.GetEnvironmentVariable("SALESFORCE_TOKEN_ENDPOINT") ?? "";
        private static readonly string ApiClientKey = Environment.GetEnvironmentVariable("SALESFORCE_CLIENT_KEY") ?? "";
        private static readonly string ApiClientSecret = Environment.GetEnvironmentVariable("SALESFORCE_CLIENT_SECRET") ?? "";
        private static string? ApiAccessToken;
        private static string ApiPath = "/services/data/v56.0/sobjects";

        /*
        * Opportunity field reference:
        * https://developer.salesforce.com/docs/atlas.en-us.240.0.object_reference.meta/object_reference/sforce_api_objects_opportunity.htm
        */

        /*
        * Opportunity update documentation:
        * https://developer.salesforce.com/docs/atlas.en-us.api_rest.meta/api_rest/dome_update_fields.htm
        */

        /* {
            Token endpoint request:

            POST /services/oauth2/token HTTP/1.1
            Host: MyDomainName.my.salesforce.com
            grant_type=client_credentials&
            client_id=*******************&
            client_secret=*******************

            Token endpoint response:

            "access_token": "*******************",
            "instance_url": "https://yourInstance.salesforce.com",
            "id": "https://login.salesforce.com/id/XXXXXXXXXXXXXXXXXX/XXXXXXXXXXXXXXXXXX",
            "token_type": "Bearer",
            "scope": "id api",
            "issued_at": "1657741493799",
            "signature": "c2lnbmF0dXJl"
        } */

        public async static Task RefreshAccessToken(ILogger _logger) {
            if (!string.IsNullOrEmpty(ApiAccessToken))
                return;
            var tokenResponse = await PostFormData<SalesForceOAuthResponse>(
                Utils.CreateUrl(
                    ApiHost,
                    ApiTokenEndpoint,
                    query:null),
                _logger,
                formData: new () { 
                    { "grant_type", "client_credentials" },
                    { "client_id", ApiClientKey },
                    { "client_secret", ApiClientSecret }
                }
            );
            _logger.LogInformation($"Got token issued_at {tokenResponse?.issued_at}");
            if (!string.IsNullOrEmpty(tokenResponse?.access_token))
                ApiAccessToken = tokenResponse.access_token;
        }

        public async static Task<HttpResponseMessage> CreateCase(
            string opportunityId,
            string teamTailorCandidateLink
            , ILogger _logger) 
        {
            var sfCase = new SalesforceCase() {
                OpportunityId = opportunityId,
                Description = "Denna ansökan är skapad via Magellos TeamTailor-integration",
                TeamTailorLink = teamTailorCandidateLink
            };
            _logger.LogInformation($"Creating new case in Salesforce: {sfCase}");
            return await Post<SalesforceCase>($"{ApiPath}/Case", null, sfCase, _logger);
        }

        public async static Task<HttpResponseMessage> UpdateOpportunity(SalesForceJob job, ILogger _logger) {
            return await Patch<SalesForceJob>($"Opportunity/{job.Id}", null, job, _logger);
        }

        private async static Task<T?> PostFormData<T>(
            string url, 
            ILogger _logger, 
            Dictionary<string, string> formData) 
        {
            _logger.LogInformation($"Calling Post {url}");
            var result = new List<T>();
            using HttpClient client = new ();
            InitClient(client);
            var content = new FormUrlEncodedContent(formData);
            var response = await client.PostAsync(url, content);
            var responseBody = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseBody, Utils.GetJsonSerializer());
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
                "Token", $"Bearer {ApiAccessToken}"
            );
        }
    }

}