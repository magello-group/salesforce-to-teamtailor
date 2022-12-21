using System.Text.Json;
using Scriban;
using Scriban.Runtime;

namespace Magello {

    public static class Utils {

        public static TeamTailorPicture GetRandomPicture() {
            var uuidPool = new List<string>() {
                "0532ef7f-f9a1-4255-b208-f360df980e86",
                "3080d4db-152f-4393-8ed8-4604e225fa9d",
                "095d9a04-df01-4d3f-be13-d4a28aca4626",
                "5ce6eea7-1856-43bd-977f-b71056256585",
                "00300a2b-6a56-42a7-85d1-dd0024889e2b",
                "0de870ea-3ab1-4de1-aad2-e9620234bab4"
            };
            var uuid = uuidPool[new Random().Next(0, uuidPool.Count - 1)];
            var pic = new TeamTailorPicture() {
                Original = $"https://teamtailor-production.s3.eu-west-1.amazonaws.com/image_uploads/{uuid}/original.jpg",
                Standard = $"https://media.cdn.teamtailor.com/images/s3/teamtailor-production/hero_picture_large-v6/image_uploads/{uuid}/original.jpg",
                Thumb = $"https://media.cdn.teamtailor.com/images/s3/teamtailor-production/user_picture_800-v8/image_uploads/{uuid}/original.jpg"
            };
            return pic;
        }

        public static string TemplateTeamTailorBody(SalesForceJob job) {
            var templatePath = "src/templates/teamtailor-body.scriban-html";
            var template = Template.Parse(File.ReadAllText(templatePath));
            var so = new ScriptObject();
            so.Add("Name", job.Name);
            so.Add("WorkPlace", job.WorkPlace);
            so.Add("AgreementPeriod", job.AgreementPeriod);
            so.Add("Description", job.Description);
            so.Add("Now", DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
            var context = new TemplateContext();
            context.PushGlobal(so);
            return template.Render(context);
        }

        public static JsonSerializerOptions GetJsonSerializer() {
            return new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
            };
        }

        public static string CreateUrl(
            string host,
            string endpoint, 
            Dictionary<string, string>? query) 
        {
            var uri = new UriBuilder();
            uri.Scheme = "https";
            uri.Host = host;
            if (!endpoint.StartsWith("/"))
                endpoint = $"/{endpoint}";
            uri.Path = endpoint;
            if (query != null) {
                uri.Query = string.Join(
                    "&",
                    query.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={kvp.Value}")
                );
            }
            return uri.Uri.AbsoluteUri;
        }

    }


}