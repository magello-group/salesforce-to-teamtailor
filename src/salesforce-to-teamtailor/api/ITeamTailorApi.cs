using System.Text.Json.Nodes;

namespace Magello.SalesforceToTeamtailor.Api;

public interface ITeamTailorApi
{
    Task<HttpResponseMessage> CreateCustomFieldMappings(JsonNode values);
    Task<HttpResponseMessage> CreateJob(JsonNode job);
    Task<JsonArray> GetApplications(DateTime since);
    Task<JsonNode?> GetCandidateFromApplication(JsonNode application);
    Task<JsonNode?> GetCustomFields();
    Task<Dictionary<string, string>?> GetCustomFieldValues(JsonNode job);
    Task<JsonNode?> GetJobFromApplication(JsonNode application);
}
