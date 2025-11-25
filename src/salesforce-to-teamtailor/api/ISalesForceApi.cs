
namespace Magello.SalesforceToTeamtailor.Api;

public interface ISalesForceApi
{
    Task CreateCase(string opportunityId, string teamTailorCandidateLink);
}