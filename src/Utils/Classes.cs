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

    // API representation of a job in Team Tailor
    public class TeamTailorJob {
        public TeamTailorJobData Data { get; set; } = new ();

        public override string ToString()
        {
            return JsonSerializer.Serialize<TeamTailorJob>(this, Utils.GetJsonSerializer());
        }
    }

    public class TeamTailorJobs {
        public List<TeamTailorJobData> Data { get; set; } = new();
        public TeamTailorPaginationLinks? Links { get; set; }
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
        public TeamTailorJobLinks? Links { get; set; }
        public TeamTailorRelationships Relationships { get; set; } = new ();
    }

    public class TeamTailorJobLinks {

    }

    public class TeamTailorJobAttributes {
        public string? Title { get; set; }
        public string? Body { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string>? Tags { get; set; }
        // The job's status, can be one of the following: open, draft, archived, unlisted or temp
        public string Status { get; set; } = "draft";
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? SalesForceOpportunityId { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public TeamTailorPicture? Picture { get; set; }
    }

    public class TeamTailorPicture {
        public string? Original { get; set; }
        public string? Standard { get; set; }
        public string? Thumb { get; set; }
    }

    public class TeamTailorRelationships {
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