namespace Magello {

public static class Mappings {

        public static TeamTailorJob SalesForceToTeamTailor(SalesForceJob sfJob) {
            TeamTailorJob ttJob = new ();

            ttJob.Data.Attributes.Title = sfJob.Name;
            ttJob.Data.Attributes.Body = sfJob.Description;
            ttJob.Data.Relationships.User = new TeamTailorUsers();
            ttJob.Data.Relationships.User.Data.Id = int.Parse(sfJob.TeamTailorUserId.Replace(" ", ""));

            // Add tags to be able to later filter jobs added by this integration
            ttJob.Data.Attributes.Tags = new List<string>();
            ttJob.Data.Attributes.Tags.Add("salesforce");
            ttJob.Data.Attributes.Tags.Add($"sfid:{sfJob.Id}");

            return ttJob;
        }

}

}