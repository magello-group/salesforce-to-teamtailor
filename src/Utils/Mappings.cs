using System.Text.Json.Nodes;

namespace Magello {

public static class Mappings {

        public static readonly string SfRefTagPrefix = "sfref:";

        public static JsonNode SalesForceToTeamTailor(SalesForceJob sfJob) {
            var ttJob = new JsonObject();
            if (sfJob == null || sfJob.TeamTailorUserId == null)
                return ttJob;

            var data = new JsonObject();
            data["type"] = "jobs";
            
            var attributes = new JsonObject();
            attributes["title"] = sfJob.Name;
            attributes["body"] = Utils.TemplateTeamTailorBody(sfJob);
            attributes["picture"] = Utils.GetRandomPictureUrl();
            attributes["status"] = "draft";
            attributes["tags"] = new JsonArray(
                "salesforce",
                $"{sfJob.InternalRefNr}"
            );

            var relationships = new JsonObject();
            var user = new JsonObject();
            var userData = new JsonObject();
            userData["type"] = "users";
            userData["id"] = int.Parse(sfJob.TeamTailorUserId.Replace(" ", ""));
            user["data"] = userData;
            relationships["user"] = user;

            data["attributes"] = attributes;
            data["relationships"] = relationships;
            ttJob["data"] = data;

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