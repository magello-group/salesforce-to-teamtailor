using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Azure.Identity;
using Scriban;
using Scriban.Runtime;

namespace Magello.SalesforceToTeamtailor.Utils;

public static class Utils
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    public static DefaultAzureCredential AzureCredentials { get; } = new(new DefaultAzureCredentialOptions
    {
        // Exclude these for all build configurations
        ExcludeEnvironmentCredential = true,
        ExcludeWorkloadIdentityCredential = true,
        ExcludeBrokerCredential = true,

#if DEBUG
        // Development exclusion overrides
        ExcludeManagedIdentityCredential = true
#else
    // Production exclusion overrides
    ExcludeAzureCliCredential = true,
    ExcludeAzureDeveloperCliCredential = true,
    ExcludeAzurePowerShellCredential = true,
    ExcludeVisualStudioCredential = true,
    ExcludeVisualStudioCodeCredential = true
#endif
    });

    public static string JsonNodeToString(JsonNode node)
    {
        return node.ToJsonString(_jsonOptions);
    }

    public static string? GetNextLink(JsonNode? node)
    {
        if (node?["links"]?["next"]?.GetValue<string>() == null)
        {
            return null;
        }

        return node["links"]!["next"]!.GetValue<string>();
    }

    public static string GetRandomPictureUrl()
    {
        var uuidPool = new List<string>()
        {
            "0532ef7f-f9a1-4255-b208-f360df980e86",
            "3080d4db-152f-4393-8ed8-4604e225fa9d",
            "095d9a04-df01-4d3f-be13-d4a28aca4626",
            "5ce6eea7-1856-43bd-977f-b71056256585",
            "00300a2b-6a56-42a7-85d1-dd0024889e2b",
            "0de870ea-3ab1-4de1-aad2-e9620234bab4"
        };
        var uuid = uuidPool[new Random().Next(0, uuidPool.Count - 1)];
        return $"https://teamtailor-production.s3.eu-west-1.amazonaws.com/image_uploads/{uuid}/original.jpg";
    }

    public static string TemplateTeamTailorBody(SalesForceJob job)
    {
        const string templatePath = "src/templates/teamtailor-body.scriban-html";
        var template = Template.Parse(File.ReadAllText(templatePath));
        var so = new ScriptObject();
        // Parse the last answer date part, which is sent like:
        // 2000-01-01
        // We'd rather do the parsing here instead of doing formatting in SF
        var lastAnswerDatePart = job.LastAnswerDatePart;

        var tryParseExact = DateTime.TryParseExact(
            lastAnswerDatePart,
            "yyyy-MM-dd",
            new CultureInfo("en-US"),
            DateTimeStyles.None,
            out var lastAnswerDatePartDate);

        so.Add("Name", job.Name);
        so.Add("WorkPlace", job.WorkPlace);
        so.Add("AgreementPeriod", job.AgreementPeriod);
        so.Add("Description", job.Description?.Replace(Environment.NewLine, "\n"));
        so.Add("InternalRefNr", job.InternalRefNr);
        so.Add("Now", DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
        so.Add("Extent", job.Extent);
        so.Add("LastAnswerDatePart",
            tryParseExact
                ? lastAnswerDatePartDate.ToString("yyyy-MM-dd")
                : "Misslyckades att hantera datumet från Salesforce");
        so.Add("Requirements", job.Requirements?.Split(Environment.NewLine));
        so.Add("ExtraRequirements", job.ExtraRequirements?.Split(Environment.NewLine));
        var context = new TemplateContext();
        context.PushGlobal(so);
        return template.Render(context);
    }

    public static JsonSerializerOptions GetJsonSerializer()
    {
        return _jsonOptions;
    }

    public static string CreateUrl(
        string host,
        string endpoint,
        Dictionary<string, string>? query = null)
    {
        var uri = new UriBuilder
        {
            Scheme = "https",
            Host = host
        };
        if (!endpoint.StartsWith("/"))
        {
            endpoint = $"/{endpoint}";
        }

        uri.Path = endpoint;
        if (query != null)
        {
            uri.Query = string.Join(
                "&",
                query.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={kvp.Value}")
            );
        }

        return uri.Uri.AbsoluteUri;
    }
}