using System.Text.Json;
using System.Text.Json.Serialization;

namespace Magello {

    // API representation of a job in SalesForce
    public class SalesForceJob {
        public string? Id { get; set; }
        public string? TeamTailorUserId { get; set; }
        public string? Name { get; set; }
        public string? AccountName { get; set; }
        public string? LastAnswerDatePart { get; set; }
        public string? AgreementPeriod { get; set; }
        public string? WorkPlace { get; set; }
        public string? Description { get; set; }
        public string? InternalRefNr { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize<SalesForceJob>(this, Utils.GetJsonSerializer());
        }
    }

    public class SalesforceCase {

        public string Status { get; set; } = "Ny";
        public string Subject { get; set; } = "Ansökan via TeamTailor";
        public string? Description { get; set; }
        [JsonPropertyName("Opportunity__c")]
        public string? OpportunityId { get; set; }
        [JsonPropertyName("TeamTailorLink__c")]
        public string? TeamTailorLink { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize<SalesforceCase>(this, Utils.GetJsonSerializer());
        }
    }

    // Expected response for SalesForce
    public class SalesForceResponse {
        public string? Id { get; set; }
        public string? Status { get; set; }
        public string? Link { get; set; }
    }

    public class SalesForceOAuthResponse {
        public string? access_token { get; set; }
        public string? instance_url { get; set; }
        public string? id { get; set; } 
        public string? token_type { get; set; }
        public string? scope { get; set; }
        public string? issued_at { get; set; }
        public string? signature { get; set; }
    }

}