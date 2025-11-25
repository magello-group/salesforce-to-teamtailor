using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Magello.SalesforceToTeamtailor.Api;
using Magello.SalesforceToTeamtailor.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Magello.SalesforceToTeamtailor.Functions;

public class SalesForceHttpFunction
{
    private readonly ILogger _logger;
    private readonly ITeamTailorApi _teamTailorAPI;
    private readonly IConfiguration _configuration;

    public SalesForceHttpFunction(ITeamTailorApi teamTailorAPI, IConfiguration configuration, ILogger<SalesForceHttpFunction> logger)
    {
        _teamTailorAPI = teamTailorAPI;
        _configuration = configuration;
        _logger = logger;
    }

    [Function("SalesForceHttpFunction")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "POST")] HttpRequestData req)
    {
        _logger.LogInformation("PostFromSalesForce function processing a request..");

        SalesForceJob? sfData;
        try
        {
            sfData = await JsonSerializer.DeserializeAsync<SalesForceJob>(req.Body, Utils.Utils.GetJsonSerializer());
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error when deserializing json body");
            var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await errorResponse.WriteAsJsonAsync(new SalesForceResponse { Status = "error" });
            return errorResponse;
        }

        if (sfData == null)
        {
            _logger.LogError("Salesforce data was null");
            var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await errorResponse.WriteAsJsonAsync(new SalesForceResponse { Status = "error" });
            return errorResponse;
        }

        _logger.LogInformation("Received data: {SfData}", sfData);

        var teamTailorJob = Mappings.SalesForceToTeamTailor(sfData);
        _logger.LogInformation("After mapping: {TeamTailorJob}", teamTailorJob);
        var apiResponse = await _teamTailorAPI.CreateJob(teamTailorJob);
        var content = await apiResponse.Content.ReadAsStringAsync();

        if (!apiResponse.IsSuccessStatusCode)
        {
            _logger.LogError("Bad response from TT API for CreateJob: {Content}", content);
            var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await errorResponse.WriteAsJsonAsync(new SalesForceResponse { Status = "error" });
            return errorResponse;
        }

        var createdJob = JsonSerializer.Deserialize<JsonNode>(content, Utils.Utils.GetJsonSerializer());

        if (createdJob == null)
        {
            _logger.LogError("Unable to deserialize TeamTailor job: {Content}", content);
            var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await errorResponse.WriteAsJsonAsync(new SalesForceResponse { Status = "error" });
            return errorResponse;
        }

        // Add custom field values to the job
        var customFieldValues = Mappings.CreateCustomFieldValues(sfData, createdJob, _configuration);
        if (customFieldValues == null)
        {
            _logger.LogError("Unable to create custom field values: {Content}", content);
            var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await errorResponse.WriteAsJsonAsync(new SalesForceResponse { Status = "error" });
            return errorResponse;
        }

        _logger.LogInformation("Custom value json: {CustomFieldValues}", Utils.Utils.JsonNodeToString(customFieldValues));
        apiResponse = await _teamTailorAPI.CreateCustomFieldMappings(customFieldValues);
        content = await apiResponse.Content.ReadAsStringAsync();
        if (!apiResponse.IsSuccessStatusCode)
        {
            _logger.LogError("Bad response from TT API for CreateCustomFieldMappings: {Content}", content);
            var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await errorResponse.WriteAsJsonAsync(new SalesForceResponse { Status = "error" });
            return errorResponse;
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        var jobUrl = createdJob["data"]?["links"]?["careersite-job-url"]?.GetValue<string>();
        _logger.LogInformation("Returning career site link {JobUrl}", jobUrl);
        await response.WriteAsJsonAsync(new SalesForceResponse
        {
            Id = sfData.Id,
            Link = jobUrl
        });
        return response;
    }
}
