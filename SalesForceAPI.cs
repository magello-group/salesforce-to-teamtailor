using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace  Magello
{
    
    /*
    * To get an access token for Salesforce, use this guide:
    * https://developer.salesforce.com/docs/atlas.en-us.api_rest.meta/api_rest/quickstart_oauth.htm
    */
    public static class SalesForceApi {

        private static readonly string SalesForceApiHost = "r2m.my.salesforce.com";
        private static readonly string ApiToken = Environment.GetEnvironmentVariable("SALESFORCE_API_TOKEN");

    }

}