using System.Text.Json;
using Azure;
using Azure.Data.Tables;
using Magello.SalesforceToTeamtailor.Api;
using Magello.SalesforceToTeamtailor.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Magello.SalesforceToTeamtailor.Functions;

public class TeamTailorTimerFunction
{
    private const string StorageTableName = "Applications";

    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;
    private readonly ISalesForceApi _salesForceApi;
    private readonly ITeamTailorApi _teamTailorAPI;

    public TeamTailorTimerFunction(ISalesForceApi salesForceApi, ITeamTailorApi teamTailorAPI, 
                                   ILogger<TeamTailorTimerFunction> logger, IConfiguration configuration)
    {
        _salesForceApi = salesForceApi;
        _teamTailorAPI = teamTailorAPI;
        _logger = logger;
        _configuration = configuration;
    }

    [Function("TeamTailorTimerFunction")]
    public async Task Run([TimerTrigger("0 */5 8-20 * * Mon-Fri", RunOnStartup = true)] MyInfo myTimer)
    {
        var salesForceCustomFieldId = _configuration.GetValue<string>(Envs.E_SalesForceCustomFieldId) ??
            throw new InvalidOperationException($"{Envs.E_SalesForceCustomFieldId} not set in configuration");

        _logger.LogInformation($"TeamTailorTimerFunction executed at: {DateTime.Now}");
        _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus?.Next}");

        // Get the datetime of our last run
        var lastRun = DateTime.Now;
        if (myTimer.ScheduleStatus?.Last != null)
        {
            lastRun = myTimer.ScheduleStatus.Last;
        }

        _logger.LogInformation($"Last run was at {lastRun}");

        // Get applications created since date of last run
        var applications = await _teamTailorAPI.GetApplications(lastRun);

        _logger.LogInformation($"Found {applications.Count} applications since last run");

        // No applications - we're done
        if (applications.Count == 0)
        {
            return;
        }

        // Get team tailor custom fields
        var customFields = await _teamTailorAPI.GetCustomFields();
        if (customFields == null)
        {
            _logger.LogError("Custom fields was null");
            return;
        }

        // Get an Azure Table Storage client
        var tableClient = await GetTableClient(_logger);

        // Loop all found applications
        foreach (var application in applications)
        {
            // Sanity check
            if (application == null)
            {
                _logger.LogInformation("Application was null");
                continue;
            }

            // Get linked job for application
            var job = await _teamTailorAPI.GetJobFromApplication(application);
            if (job == null)
            {
                _logger.LogInformation("Job was null");
                continue;
            }

            // Get custom field values
            var fieldValues = await _teamTailorAPI.GetCustomFieldValues(job);
            if (fieldValues == null || fieldValues.Count == 0)
            {
                _logger.LogInformation("Job has no custom field values");
                continue;
            }

            if (!fieldValues.TryGetValue(salesForceCustomFieldId, out var opportunityId))
            {
                _logger.LogInformation("Job has no custom field value for salesforce id");
                continue;
            }

            // Try to get saved application
            var existingApplication = tableClient.Query<ApplicationTableEntity>(e =>
                e.ApplicationId == application["id"]!.GetValue<string>()
            ).FirstOrDefault();

            if (existingApplication != null)
            {
                // This application has already been processed
                _logger.LogInformation("Application with id {ApplicationId} already processed",
                                       application["id"]!.GetValue<string>());

                continue;
            }

            var candidate = await _teamTailorAPI.GetCandidateFromApplication(application);

            if (candidate == null)
            {
                _logger.LogInformation("Could not get candidate from application");
                continue;
            }

            // Add the application to table storage
            var newTableEntity = new ApplicationTableEntity()
            {
                ApplicationId = application["id"]!.GetValue<string>(),
                RowKey = application["id"]!.GetValue<string>()
            };
            _logger.LogInformation("Added new case to table storage: {NewTableEntity}", newTableEntity);
            tableClient.AddEntity(newTableEntity);

            // Create Salesforce case for application
            var jobId = job["data"]!["id"]!;
            var candidateId = candidate["data"]!["id"]!;
            var teamTailorUrl = _configuration.GetValue<string>(Envs.E_TeamTailorBaseUrl) ??
                throw new InvalidOperationException($"{Envs.E_TeamTailorBaseUrl} not set in configuration");

            var teamTailorCandidateLink = $"{teamTailorUrl}/jobs/{jobId}/stages/candidate/{candidateId}";
            await _salesForceApi.CreateCase(opportunityId, teamTailorCandidateLink);
        }
    }

    private async Task<TableClient> GetTableClient(ILogger _logger)
    {
        var storageAccountUri = _configuration.GetValue<string>(Envs.E_AzStorageAccountUri) ??
                throw new InvalidOperationException($"{Envs.E_AzStorageAccountUri} not set in configuration");

        _logger.LogDebug("Getting Storage Table client.");

        var tableClient = new TableClient(new Uri(storageAccountUri), StorageTableName, Utils.Utils.AzureCredentials);

        await tableClient.CreateIfNotExistsAsync();

        _logger.LogDebug("TableClient OK");

        return tableClient;
    }
}

public class ApplicationTableEntity : ITableEntity
{
    public string ApplicationId { get; set; } = "";
    public string PartitionKey { get; set; } = "";
    public string RowKey { get; set; } = "";
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public override string ToString()
    {
        return JsonSerializer.Serialize<ApplicationTableEntity>(this, Utils.Utils.GetJsonSerializer());
    }
}

public class MyInfo
{
    public MyScheduleStatus? ScheduleStatus { get; set; }

    public bool IsPastDue { get; set; }
}

public class MyScheduleStatus
{
    public DateTime Last { get; set; }

    public DateTime Next { get; set; }

    public DateTime LastUpdated { get; set; }
}
