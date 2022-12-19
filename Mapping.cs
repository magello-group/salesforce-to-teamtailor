namespace Magello {

public static class Mappings {

        public static TeamTailorJob SalesForceToTeamTailor(SalesForceJob sfJob) {
            TeamTailorJob ttJob = new ();

            ttJob.Data.Attributes.Title = sfJob.Name;
            ttJob.Data.Attributes.Body = sfJob.Description;
            ttJob.Data.Relationships.User = new TeamTailorUsers();
            ttJob.Data.Relationships.User.Data.Id = int.Parse(sfJob.TeamTailorUserId.Replace(" ", ""));

            return ttJob;
        }

}

}