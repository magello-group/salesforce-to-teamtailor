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
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Function, "PUT", "POST", "DELETE")] HttpRequestData req)
        {
            _logger.LogInformation("PostFromSalesForce function processing a request..");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString("Welcome to Azure Functions!");

            return response;
        }
    }
}
