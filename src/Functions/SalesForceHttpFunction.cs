using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Magello.SalesForceHttpFunction
{
    public class SalesForceHttpFunction
    {
        private readonly ILogger _logger;

        public SalesForceHttpFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<SalesForceHttpFunction>();
            Envs.PreFlightEnvChecks();
        }

        [Function("SalesForceHttpFunction")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "POST")] HttpRequestData req)
        {
            _logger.LogInformation("PostFromSalesForce function processing a request..");

            SalesForceJob? sfData;
            try
            {
                sfData = await JsonSerializer.DeserializeAsync<SalesForceJob>(
                    req.Body,
                    Utils.GetJsonSerializer()
                );
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
            var apiResponse = await TeamTailorAPI.CreateJob(teamTailorJob, _logger);
            var content = await apiResponse.Content.ReadAsStringAsync();

            if (!apiResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Bad response from TT API for CreateJob: {Content}", content);
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new SalesForceResponse { Status = "error" });
                return errorResponse;
            }

            var createdJob = JsonSerializer.Deserialize<JsonNode>(content, Utils.GetJsonSerializer());

            if (createdJob == null)
            {
                _logger.LogError("Unable to deserialize TeamTailor job: {Content}", content);
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new SalesForceResponse { Status = "error" });
                return errorResponse;
            }

            // Add custom field values to the job
            var customFieldValues = Mappings.CreateCustomFieldValues(sfData, createdJob);
            if (customFieldValues == null)
            {
                _logger.LogError("Unable to create custom field values: {Content}", content);
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new SalesForceResponse { Status = "error" });
                return errorResponse;
            }

            _logger.LogInformation("Custom value json: {CustomFieldValues}", Utils.JsonNodeToString(customFieldValues));
            apiResponse = await TeamTailorAPI.CreateCustomFieldMappings(customFieldValues, _logger);
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
}