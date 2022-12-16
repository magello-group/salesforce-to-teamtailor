using System.Net;
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

            SalesForceData jsonData;
            try {
                jsonData = await System.Text.Json.JsonSerializer.DeserializeAsync<SalesForceData>(req.Body);
            }
            catch (Exception e) {
                _logger.LogError(e, "Error when deserializing json body");
                var errorResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await errorResponse.WriteAsJsonAsync(new ErrorData() { error = "bad json data provided "});
                return errorResponse;
            }

            _logger.LogInformation($"Recieved data: {jsonData}");

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(jsonData);
            return response;
        }

        private class SalesForceData {
            public string? id { get; set; }
            public string? name { get; set; }
            public string? accountName { get; set; }
            public string? lastAnswerDatePart { get; set; }
            public string? agreementPeriod { get; set; }
            public string? workPlace { get; set; }
            public string? description { get; set; }

            public override string ToString()
            {
                return $"id: {id}, name: {name}";
            }
        }

        private class ErrorData {
            public string? error { get; set; }
        }
    }
}
