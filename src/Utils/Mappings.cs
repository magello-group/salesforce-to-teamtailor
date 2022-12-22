namespace Magello {

public static class Mappings {

        public static readonly string SfRefTagPrefix = "sfref:";

        public static TeamTailorJob SalesForceToTeamTailor(SalesForceJob sfJob) {
            TeamTailorJob ttJob = new ();

            if (sfJob == null)
                return ttJob;

            ttJob.Data.Attributes.Title = sfJob.Name;
            ttJob.Data.Attributes.Body = Utils.TemplateTeamTailorBody(sfJob);
            ttJob.Data.Attributes.Picture = Utils.GetRandomPictureUrl();
            ttJob.Data.Relationships.User = new TeamTailorUsers();
            if (sfJob.TeamTailorUserId != null)
                ttJob.Data.Relationships.User.Data.Id = int.Parse(sfJob.TeamTailorUserId.Replace(" ", ""));

            // Add tags to be able to later filter jobs added by this integration
            ttJob.Data.Attributes.Tags = new List<string>();
            ttJob.Data.Attributes.Tags.Add("salesforce");
            ttJob.Data.Attributes.Tags.Add($"{SfRefTagPrefix}{sfJob.InternalRefNr}");

            return ttJob;
        }

}

}