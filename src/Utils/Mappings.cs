using System.Text.Json.Nodes;

namespace Magello
{
    public static class Mappings
    {
        public static readonly string SfRefTagPrefix = "sfref:";

        public static JsonNode SalesForceToTeamTailor(SalesForceJob sfJob)
        {
            var ttJob = new JsonObject();
            if (sfJob.TeamTailorUserId == null)
                return ttJob;

            var data = new JsonObject
            {
                ["type"] = "jobs"
            };

            var now = DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffK");
            var attributes = new JsonObject
            {
                ["title"] = sfJob.Name,
                ["body"] = Utils.TemplateTeamTailorBody(sfJob),
                ["picture"] = Utils.GetRandomPictureUrl(),
                ["status"] = "draft",
                ["created-at"] = now,
                ["updated-at"] = now,
                ["resume-requirement"] = "required",
                ["cover-letter-requirement"] = "off",
                ["apply-button-text"] = "ANSÖK HÄR",
                ["tags"] = new JsonArray(
                    "salesforce",
                    $"{sfJob.InternalRefNr}"
                )
            };

            var userData = new JsonObject
            {
                ["type"] = "users",
                ["id"] = int.Parse(sfJob.TeamTailorUserId.Replace(" ", ""))
            };

            var user = new JsonObject
            {
                ["data"] = userData
            };

            // 1145804 is 'Vilket är ditt prisförslag'
            // 1136520 is 'Ange konsultens tillgänglighet'
            var questionData = new JsonObject
            {
                ["data"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["id"] = "1145804",
                        ["type"] = "questions"
                    },
                    new JsonObject
                    {
                        ["id"] = "1136520",
                        ["type"] = "questions"
                    }
                }
            };

            var relationships = new JsonObject
            {
                ["user"] = user,
                ["questions"] = questionData
            };

            data["attributes"] = attributes;
            data["relationships"] = relationships;
            ttJob["data"] = data;

            return ttJob;
        }

        public static JsonNode? CreateCustomFieldValues(
            SalesForceJob? sfJob,
            JsonNode? ttJob)
        {
            var fieldValues = new JsonObject();

            if (sfJob == null || ttJob == null)
                return null;

            // Get the id of the newly created job in team tailor
            var ttJobId = ttJob["data"]["id"].GetValue<string>();

            // Add data root object
            var data = new JsonObject();
            data["type"] = "custom-field-values";

            // Add custom field value to object
            var attributes = new JsonObject();
            attributes["value"] = sfJob.Id;
            data["attributes"] = attributes;

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