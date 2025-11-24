using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Magello.SalesforceToTeamtailor.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Magello.SalesforceToTeamtailor.Api;


/*
* Salesforce asnd oauth:
* https://developer.salesforce.com/docs/atlas.en-us.api_rest.meta/api_rest/quickstart_oauth.htm
* https://developer.salesforce.com/docs/atlas.en-us.api_rest.meta/api_rest/intro_oauth_and_connected_apps.htm
*/
public static class SalesForceApi
{

    private static string? ApiAccessToken;
    private static readonly string ApiPath = "/services/data/v56.0/sobjects";

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

    public static async Task RefreshAccessToken(IConfiguration configuration, ILogger logger)
    {
        if (!string.IsNullOrEmpty(ApiAccessToken))
        {
            return;
        }

        var salesForceApiHost = configuration.GetValue<string>(Envs.E_SalesForceApiHost) ??
            throw new InvalidOperationException($"{Envs.E_SalesForceApiHost} not set in configuration");

        var salesForceApiTokenEndpoint = configuration.GetValue<string>(Envs.E_SalesForceApiTokenEndpoint) ??
            throw new InvalidOperationException($"{Envs.E_SalesForceApiTokenEndpoint} not set in configuration");

        var salesForceApiClientKey = configuration.GetValue<string>(Envs.E_SalesForceApiClientKey) ??
            throw new InvalidOperationException($"{Envs.E_SalesForceApiClientKey} not set in configuration");

        var salesForceApiClientSecret = configuration.GetValue<string>(Envs.E_SalesForceApiClientSecret) ??
            throw new InvalidOperationException($"{Envs.E_SalesForceApiClientSecret} not set in configuration");

        var tokenResponse = await PostFormData<SalesForceOAuthResponse>(
            Utils.Utils.CreateUrl(salesForceApiHost, salesForceApiTokenEndpoint),
            logger,
            formData: new() {
                { "grant_type", "client_credentials" },
                { "client_id", salesForceApiClientKey },
                { "client_secret", salesForceApiClientSecret }
            }
        );
        logger.LogInformation($"Got token issued_at {tokenResponse?.issued_at}");
        if (!string.IsNullOrEmpty(tokenResponse?.access_token))
        {
            ApiAccessToken = tokenResponse.access_token;
        }
    }

    public static async Task<HttpResponseMessage> CreateCase(
        string opportunityId,
        string teamTailorCandidateLink,
        IConfiguration configuration,
        ILogger logger)
    {
        var sfCase = new SalesforceCase()
        {
            OpportunityId = opportunityId,
            Description = "Denna ansökan är skapad via Magellos TeamTailor-integration",
            TeamTailorLink = teamTailorCandidateLink
        };
        logger.LogInformation($"Creating new case in Salesforce: {sfCase}");
        return await Post<SalesforceCase>($"{ApiPath}/Case", null, sfCase, configuration, logger);
    }

    public static async Task<HttpResponseMessage> UpdateOpportunity(SalesForceJob job,
        IConfiguration configuration, ILogger logger)
    {
        return await Patch<SalesForceJob>($"Opportunity/{job.Id}", null, job, configuration, logger);
    }

    private static async Task<T?> PostFormData<T>(
        string url,
        ILogger logger,
        Dictionary<string, string> formData)
    {
        logger.LogInformation($"Calling Post {url}");
        var result = new List<T>();
        using HttpClient client = new();
        InitClient(client);
        var content = new FormUrlEncodedContent(formData);
        var response = await client.PostAsync(url, content);
        var responseBody = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(responseBody, Utils.Utils.GetJsonSerializer());
    }

    private static async Task<HttpResponseMessage> Patch<T>(
        string endpoint,
        Dictionary<string, string>? query,
        T jsonData,
        IConfiguration configuration,
        ILogger logger)
    {
        var salesForceApiHost = configuration.GetValue<string>(Envs.E_SalesForceApiHost) ??
            throw new InvalidOperationException($"{Envs.E_SalesForceApiHost} not set in configuration");

        var url = Utils.Utils.CreateUrl(salesForceApiHost, endpoint, query);

        logger.LogInformation($"Calling PATCH {url}");
        using HttpClient client = new();
        InitClient(client);
        var request = CreateJsonDataRequest<T>(jsonData);
        return await client.PatchAsync(url, request.Content);
    }

    private static async Task<HttpResponseMessage> Post<T>(
        string endpoint,
        Dictionary<string, string>? query,
        T jsonData,
        IConfiguration configuration,
        ILogger logger)
    {
        var salesForceApiHost = configuration.GetValue<string>(Envs.E_SalesForceApiHost) ??
            throw new InvalidOperationException($"{Envs.E_SalesForceApiHost} not set in configuration");

        var url = Utils.Utils.CreateUrl(salesForceApiHost, endpoint, query);
        logger.LogInformation($"Calling POST {url}");
        using HttpClient client = new();
        InitClient(client);
        var request = CreateJsonDataRequest<T>(jsonData);
        return await client.PostAsync(url, request.Content);
    }

    private static HttpRequestMessage CreateJsonDataRequest<T>(T? jsonData)
    {
        var request = new HttpRequestMessage();
        if (jsonData != null)
        {
            var json = JsonSerializer.Serialize<T>(jsonData, Utils.Utils.GetJsonSerializer());
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            request.Content.Headers.Remove("Content-Type");
            request.Content.Headers.Add("Content-Type", "application/json");
        }
        return request;
    }

    private static void InitClient(HttpClient client)
    {
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Token", $"Bearer {ApiAccessToken}"
        );
    }
}
