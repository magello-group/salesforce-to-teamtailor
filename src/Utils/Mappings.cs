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

            // Add data root object
            var data = new JsonObject();
            data["type"] = "custom-field-values";
            var attributes = new JsonObject();
            attributes["value"] = sfJob.Id;

            var relationships = new JsonObject();
            var customField = new JsonObject();
            var customFieldData = new JsonObject();
            var owner = new JsonObject();
            var ownerData = new JsonObject();

            // Add custom field relationship
            customFieldData["id"] = int.Parse(Envs.GetEnvVar(Envs.E_SalesForceCustomFieldId));
            customFieldData["type"] = "custom-fields";
            customField["data"] = customFieldData;
            relationships["custom-field"] = customField;

            // Add owner relationship
            ownerData["type"] = "jobs";
            ownerData["id"] = int.Parse(ttJobId);
            owner["data"] = ownerData;
            relationships["owner"] = owner;

            // Add relationships to root
            data["relationships"] = relationships;

            fieldValues["data"] = data;

            return fieldValues;
        }
    }

}