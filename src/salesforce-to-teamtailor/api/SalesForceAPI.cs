using System.Text;
using System.Text.Json;
using Magello.SalesforceToTeamtailor.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Magello.SalesforceToTeamtailor.Api;


internal sealed class SalesForceApi(HttpClient client, ILogger<SalesForceApi> logger) 
    : ISalesForceApi
{

    private readonly HttpClient _client = client;
    private readonly ILogger<SalesForceApi> _logger = logger;

    /*
    * Opportunity field reference:
    * https://developer.salesforce.com/docs/atlas.en-us.240.0.object_reference.meta/object_reference/sforce_api_objects_opportunity.htm
    */

    /*
    * Opportunity update documentation:
    * https://developer.salesforce.com/docs/atlas.en-us.api_rest.meta/api_rest/dome_update_fields.htm
    */

    public async Task CreateCase(string opportunityId, string teamTailorCandidateLink)
    {
        var sfCase = new SalesforceCase()
        {
            OpportunityId = opportunityId,
            Description = "Denna ansökan är skapad via Magellos TeamTailor-integration",
            TeamTailorLink = teamTailorCandidateLink
        };

        _logger.LogInformation(
            "Creating new case in Salesforce for opportunity {OpportunityId} for candidate link {candidateLink}", 
            opportunityId, teamTailorCandidateLink);

        var response = await Post("Case", sfCase);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "Failed to create case in Salesforce for opportunity {OpportunityId}, request returned {StatusCode}",
                opportunityId, response.StatusCode);
        }
        else
        {
            _logger.LogInformation("Successfully created case in Salesforce for opportunity {OpportunityId}", 
                                   opportunityId);
        }
    }

    private async Task<HttpResponseMessage> Post<T>(string endpoint, T jsonData)
    {
        _logger.LogDebug("Calling POST {Endpoint}", endpoint);

        var json = JsonSerializer.Serialize(jsonData, Utils.Utils.GetJsonSerializer());
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await _client.PostAsync(endpoint, content);
    }
}
