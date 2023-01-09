using System.Text.Json.Nodes;

namespace Magello {

public static class Mappings {

        public static readonly string SfRefTagPrefix = "sfref:";

        public static JsonNode SalesForceToTeamTailor(SalesForceJob sfJob) {
            JsonNode ttJob = new JsonObject();
            if (sfJob == null)
                return ttJob;

            ttJob["data"] = new JsonObject();

            ttJob["data"]["type"] = "jobs";
            ttJob["data"]["attributes"] = new JsonObject();
            ttJob["data"]["attributes"]["title"] = sfJob.Name;
            ttJob["data"]["attributes"]["body"] = Utils.TemplateTeamTailorBody(sfJob);
            ttJob["data"]["attributes"]["picture"] = Utils.GetRandomPictureUrl();
            ttJob["data"]["attributes"]["status"] = "draft";
            
            // Add tags
            ttJob["data"]["attributes"]["tags"] = new JsonArray(
                "salesforce",
                $"{sfJob.InternalRefNr}"
            );
            
            ttJob["data"]["relationships"] = new JsonObject();
            ttJob["data"]["relationships"]["user"] = new JsonObject();
            ttJob["data"]["relationships"]["user"]["data"] = new JsonObject();
            ttJob["data"]["relationships"]["user"]["data"]["type"] = "users";
            ttJob["data"]["relationships"]["user"]["data"]["id"] = 
                int.Parse(sfJob.TeamTailorUserId.Replace(" ", ""));

            return ttJob;
        }

        public static JsonNode? CreateCustomFieldValues(
            SalesForceJob sfJob, 
            JsonNode ttJob) 
        {
            var fieldValues = new JsonObject();

            if (sfJob == null || ttJob == null)
                return null;

            // Get the id of the newly created job in team tailor
            var ttJobId = ttJob["data"]["id"].GetValue<string>();

            fieldValues["data"] = new JsonObject();
            fieldValues["data"]["type"] = "custom-field-values";
            fieldValues["data"]["attributes"] = new JsonObject();
            fieldValues["data"]["attributes"]["value"] = sfJob.Id;

            fieldValues["data"]["relationships"] = new JsonObject();
            fieldValues["data"]["relationships"]["custom-field"] = new JsonObject();
            fieldValues["data"]["relationships"]["custom-field"]["data"] = new JsonObject();
            fieldValues["data"]["relationships"]["custom-field"]["data"]["id"] = 
                int.Parse(Envs.GetEnvVar(Envs.E_SalesForceCustomFieldId));
            fieldValues["data"]["relationships"]["custom-field"]["data"]["type"] = "custom-fields";
            fieldValues["data"]["relationships"]["owner"] = new JsonObject();
            fieldValues["data"]["relationships"]["owner"]["data"] = new JsonObject();
            fieldValues["data"]["relationships"]["owner"]["data"]["id"] = int.Parse(ttJobId);
            fieldValues["data"]["relationships"]["owner"]["data"]["type"] = "jobs";

            return fieldValues;
        }
    }

}