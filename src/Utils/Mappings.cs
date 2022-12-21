namespace Magello {

public static class Mappings {

        public static readonly string SfIdTagPrefix = "sfid:";

        public static TeamTailorJob SalesForceToTeamTailor(SalesForceJob sfJob) {
            TeamTailorJob ttJob = new ();

            if (sfJob == null)
                return ttJob;

            ttJob.Data.Attributes.Title = sfJob.Name;
            ttJob.Data.Attributes.Body = Utils.TemplateTeamTailorBody(sfJob);
            //ttJob.Data.Attributes.Picture = Utils.GetRandomPicture();
            ttJob.Data.Relationships.User = new TeamTailorUsers();
            if (sfJob.TeamTailorUserId != null)
                ttJob.Data.Relationships.User.Data.Id = int.Parse(sfJob.TeamTailorUserId.Replace(" ", ""));

            // Add tags to be able to later filter jobs added by this integration
            ttJob.Data.Attributes.Tags = new List<string>();
            ttJob.Data.Attributes.Tags.Add("salesforce");
            ttJob.Data.Attributes.Tags.Add($"{SfIdTagPrefix}{sfJob.Id}");

            return ttJob;
        }

}

}