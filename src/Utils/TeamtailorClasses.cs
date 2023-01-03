using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Magello {

    /*
    * CUSTOM FIELD VALUES
    */

    /*public class TeamTailorCustomFieldValues : IPageable {
        public List<TeamTailorCustomFieldValue> Data { get; set; } = new ();
        public TeamTailorPaginationLinks? Links { get; set; }

        public string? GetNextUrl()
        {
            if (Links != null && !string.IsNullOrEmpty(Links.Next))
                return Links.Next;
            return null;
        }
    }

    public class TeamTailorCustomFieldValue {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Id { get; set; } = "";
        public TeamTailorCustomFieldValueAttributes Attributes { get; set; } = new ();
    }

    public class TeamTailorCustomFieldValueAttributes {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Value { get; set; } = "";
    }*/

    /* 
    * CUSTOM FIELDS
    */

    /*public class TeamTailorCustomFields : IPageable {
        public List<TeamTailorCustomField> Data { get; set; } = new ();
        public TeamTailorPaginationLinks? Links { get; set; }

        public string? GetNextUrl()
        {
            if (Links != null && !string.IsNullOrEmpty(Links.Next))
                return Links.Next;
            return null;
        }
    }

    public class TeamTailorCustomField {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Id { get; set; } = "";
        public TeamTailorCustomFieldAttributes Attributes { get; set; } = new ();
    }

    public class TeamTailorCustomFieldAttributes {
        [JsonPropertyName("api-name")]
        public string ApiName { get; set; } = "";
    }*/

    /*
    * APPLICATIONS
    */

    /*public class TeamTailorApplications : IPageable {
        public List<TeamTailorApplicationData> Data { get; set; } = new ();
        public TeamTailorPaginationLinks? Links { get; set; }

        public string? GetNextUrl()
        {
            if (Links != null && !string.IsNullOrEmpty(Links.Next))
                return Links.Next;
            return null;
        }
    }

    public class TeamTailorApplicationData {
        public string Type { get; set; } = "job-applications";
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Id { get; set; } = "";
        public TeamTailorApplicationAttributes Attributes { get; set; } = new ();
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public TeamTailorLinks Links { get; set; } = new ();
        public TeamTailorApplicationRelationships Relationships { get; set; } = new ();
    }

    public class TeamTailorApplicationAttributes { }

    public class TeamTailorApplicationRelationships {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public TeamTailorRelation Job { get; set; } = new ();
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        // Reuse job relation, since it's the same
        public TeamTailorRelation Candidate { get; set; } = new ();
    }

    public class TeamTailorRelation {
        public TeamTailorLinks Links { get; set; } = new ();
    }*/

    /*
    * JOB (POST)
    */

    // Wrapper for single job
    /*public class TeamTailorPostJob : IPageable {
        public TeamTailorPostJobData Data { get; set; } = new ();

        public override string ToString()
        {
            return JsonSerializer.Serialize<TeamTailorPostJob>(this, Utils.GetJsonSerializer());
        }

        // Single jobs are never pageable
        public string? GetNextUrl()
        {
            return null;
        }
    }*/

    /*
    * JOB (GET)
    */

    // Wrapper for single job
    /*public class TeamTailorJob : IPageable {
        public TeamTailorJobData Data { get; set; } = new ();

        public override string ToString()
        {
            return JsonSerializer.Serialize<TeamTailorJob>(this, Utils.GetJsonSerializer());
        }

        // Single jobs are never pageable
        public string? GetNextUrl()
        {
            return null;
        }
    }

    public class TeamTailorJobs : IPageable {
        public List<TeamTailorJobData> Data { get; set; } = new();
        public TeamTailorPaginationLinks? Links { get; set; }

        public string? GetNextUrl()
        {
            if (Links != null && ! string.IsNullOrEmpty(Links.Next))
                return Links.Next;
            return null;
        }
    }

    public class TeamTailorPaginationLinks {
        public string? First { get; set; }
        public string? Next { get; set; }
        public string? Last { get; set; }
    }

    // Because Teamtailor returns one variant of "picture" and requires another when
    // posting
    public class TeamTailorPostJobData {
        public string Type { get; set; } = "jobs";
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Id { get; set; }
        public TeamTailorPostJobAttributes Attributes { get; set; } = new ();
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public TeamTailorLinks? Links { get; set; }
        public TeamTailorJobPostRelationships Relationships { get; set; } = new ();
    }

    public class TeamTailorJobData {
        public string Type { get; set; } = "jobs";
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Id { get; set; }
        public TeamTailorJobAttributes Attributes { get; set; } = new ();
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public TeamTailorLinks? Links { get; set; }
        public TeamTailorJobRelationships Relationships { get; set; } = new ();
    }

    public class TeamTailorJobRelationships {
        [JsonPropertyName("custom-field-values")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public TeamTailorJobRelationship CustomFieldValueRelationship { get; set; }
    }

    public class TeamTailorJobRelationship {
        public TeamTailorLinks Links { get; set; }
    }

    public class TeamTailorLinks {
        public string Self { get; set; } = "";
        public string Related { get; set; } = "";
        [JsonPropertyName("careersite-job-url")]
        public string CareersiteJobUrl { get; set; } = "";
    }

    public class TeamTailorPostJobAttributes {
        public string? Title { get; set; }
        public string? Body { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? Tags { get; set; }
        // The job's status, can be one of the following: open, draft, archived, unlisted or temp
        public string Status { get; set; } = "draft";
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("salesforceinternalref")]
        public string? SalesForceInternalRefId { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("salesforceid")]
        public string? SalesForceId { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Picture { get; set; }
    }

    public class TeamTailorJobAttributes {
        public string? Title { get; set; }
        public string? Body { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? Tags { get; set; }
        // The job's status, can be one of the following: open, draft, archived, unlisted or temp
        public string Status { get; set; } = "draft";
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? SalesForceInternalRefId { get; set; }
        // Opportunity id gets populated from the tags
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public string? SalesForceOpportunityId { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public TeamTailorPicture? Picture { get; set; }
    }

    public class TeamTailorPicture {
        public string? Original { get; set; }
        public string? Standard { get; set; }
        public string? Thumb { get; set; }
    }

    public class TeamTailorJobPostRelationships {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public TeamTailorPostLocations? Locations { get; set; }
        public TeamTailorPostUsers? User { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("custom-field-values")]
        public TeamTailorPostCustomFieldValues? CustomFieldValues { get; set; }
    }

    public class TeamTailorPostCustomFieldValues {
        public TeamTailorPostCustomFieldValue Data { get; set; } = new ();
    }

    public class TeamTailorPostUsers {
        public TeamTailorPostUser Data { get; set; } = new ();
    }

    public class TeamTailorPostLocations {
        public List<TeamTailorPostLocation> Data { get; set; } = new ();
    }

    public class TeamTailorPostCustomFieldValue {
        public int? Id { get; set; }
        public string Type { get; set; } = "custom-field-values";
    }

    public class TeamTailorPostLocation {
        public int? Id { get; set; }
        public string Type { get; set; } = "locations";
    }

    public class TeamTailorPostUser {
        public int? Id { get; set; }
        public string Type { get; set; } = "users";
    }*/

}