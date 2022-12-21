using System.Text.Json;
using System.Text.Json.Serialization;
using Magello;

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

    // Expected response for SalesForce
    public class SalesForceResponse {
        public string? Id { get; set; }
        public string? Status { get; set; }
    }

    public interface IPageable {
        public string? GetNextUrl();
    }

    public class TeamTailorApplications : IPageable {
        public List<TeamTailorApplicationData> Data { get; set; } = new ();
        public TeamTailorPaginationLinks? Links { get; set; }

        public string? GetNextUrl()
        {
            if (Links != null && ! string.IsNullOrEmpty(Links.Next))
                return Links.Next;
            return null;
        }
    }

    public class TeamTailorApplicationData {
        public string Type { get; set; } = "job-applications";
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Id { get; set; }
        public TeamTailorApplicationAttributes Attributes { get; set; } = new ();
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public TeamTailorLinks? Links { get; set; }
        public TeamTailorApplicationRelationships Relationships { get; set; } = new ();
    }

    public class TeamTailorApplicationAttributes {

    }

    public class TeamTailorApplicationRelationships {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public TeamTailorJobRelation? Job { get; set; }
    }

    public class TeamTailorJobRelation {
        public TeamTailorLinks? Links { get; set; }
    }

    // Wrapper for single job
    public class TeamTailorJob : IPageable {
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

    public class TeamTailorJobData {
        public string Type { get; set; } = "jobs";
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Id { get; set; }
        public TeamTailorJobAttributes Attributes { get; set; } = new ();
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public TeamTailorLinks? Links { get; set; }
        public TeamTailorJobRelationships Relationships { get; set; } = new ();
    }

    public class TeamTailorLinks {
        public string? Self { get; set; }
        public string? Related { get; set; }
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
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Picture { get; set; }
    }

    public class TeamTailorPicture {
        public string? Original { get; set; }
        public string? Standard { get; set; }
        public string? Thumb { get; set; }
    }

    public class TeamTailorJobRelationships {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public TeamTailorLocations? Locations { get; set; }
        public TeamTailorUsers? User { get; set; }
    }

    public class TeamTailorUsers {
        public TeamTailorUser Data { get; set; } = new ();
    }

    public class TeamTailorLocations {
        public List<TeamTailorLocation> Data { get; set; } = new ();
    }

    public class TeamTailorLocation {
        public int? Id { get; set; }
        public string Type { get; set; } = "locations";
    }

    public class TeamTailorUser {
        public int? Id { get; set; }
        public string Type { get; set; } = "users";
    }

}