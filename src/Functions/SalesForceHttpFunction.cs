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
        }

        [Function("SalesForceHttpFunction")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "POST")] HttpRequestData req)
        {
            _logger.LogInformation("PostFromSalesForce function processing a request..");

            SalesForceJob? sfData;
            try {
                sfData = await JsonSerializer.DeserializeAsync<SalesForceJob>(
                    req.Body, 
                    Utils.GetJsonSerializer()
                );
            }
            catch (Exception e) {
                _logger.LogError(e, "Error when deserializing json body");
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new SalesForceResponse() { Status = "error"});
                return errorResponse;
            }

            if (sfData == null) {
                _logger.LogError("Salesforce data was null");
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new SalesForceResponse() { Status = "error"});
                return errorResponse;
            }

            _logger.LogInformation($"Recieved data: {sfData}");

            var teamTailorJob = Mappings.SalesForceToTeamTailor(sfData);
            _logger.LogInformation($"After mapping: {teamTailorJob}");
            var apiResponse = await TeamTailorAPI.CreateJob(teamTailorJob, _logger);
            var content = await apiResponse.Content.ReadAsStringAsync();

            if (!apiResponse.IsSuccessStatusCode) {
                _logger.LogError($"Bad response from TT API for CreateJob: {content}");
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new SalesForceResponse() { Status = "error"});
                return errorResponse;
            }

            var createdJob = JsonSerializer.Deserialize<JsonNode>(content, Utils.GetJsonSerializer());
            
            // Add custom field values to the job
            var customFieldValues = Mappings.CreateCustomFieldValues(sfData, createdJob);
            _logger.LogInformation($"Custom value json: {Utils.JsonNodeToString(customFieldValues)}");
            apiResponse = await TeamTailorAPI.CreateCustomFieldMappings(customFieldValues, _logger);
            content = await apiResponse.Content.ReadAsStringAsync();
            if (!apiResponse.IsSuccessStatusCode) {
                _logger.LogError($"Bad response from TT API for CreateCustomFieldMappings: {content}");
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new SalesForceResponse() { Status = "error"});
                return errorResponse;
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            var jobUrl = createdJob?["data"]?["links"]?["careersite-job-url"]?.GetValue<string>();
            _logger.LogInformation($"Returning career site link {jobUrl}");
            await response.WriteAsJsonAsync(new SalesForceResponse() { 
                Id = sfData.Id,
                Link = jobUrl
            });
            return response;
        }

    }
}
