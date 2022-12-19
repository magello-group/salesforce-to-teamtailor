using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Magello.SalesForceToTeamTailor
{

    public class PostFromSalesForce
    {

        private readonly ILogger _logger;

        public PostFromSalesForce(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<PostFromSalesForce>();
        }

        [Function("PostFromSalesForce")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "PUT", "POST", "DELETE")] HttpRequestData req)
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
                _logger.LogError($"Bad response from Team Tailor API: {content}");
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new SalesForceResponse() { Status = "error"});
                return errorResponse;
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new SalesForceResponse() { Id = sfData.Id });
            return response;
        }

    }
}
