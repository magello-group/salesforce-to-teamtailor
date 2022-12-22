using System;
using Azure;
using Azure.Data.Tables;
using Azure.Data.Tables.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Magello.TeamTailorTimerFunction
{
    public class TeamTailorTimerFunction
    {

        private readonly ILogger _logger;
        private readonly string StorageAccountUri = Environment.GetEnvironmentVariable("TABLE_STORAGE_URI");
        private readonly string StorageAccountName = Environment.GetEnvironmentVariable("TABLE_STORAGE_NAME");
        private readonly string StorageAccountKey = Environment.GetEnvironmentVariable("TABLE_STORAGE_KEY");
        private readonly string StorageTableName = "Applications";

        public TeamTailorTimerFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<TeamTailorTimerFunction>();
        }

        [Function("TeamTailorTimerFunction")]
        public async Task Run([TimerTrigger("0 */5 * * * *", RunOnStartup = true)] MyInfo myTimer)
        {
            _logger.LogInformation($"TeamTailorTimerFunction executed at: {DateTime.Now}");
            _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus?.Next}");

            // Get the datetime of our last run
            var lastRun = DateTime.Now;
            if (myTimer.ScheduleStatus?.Last != null)
                lastRun = myTimer.ScheduleStatus.Last;

            _logger.LogInformation($"Last run was at {lastRun.ToString()}");

            // Get applications created since date of last run
            var applications = await TeamTailorAPI.GetApplications(lastRun, _logger);
            _logger.LogInformation($"Found {applications.Count} applications since last run");

            // No applications - we're done
            if (!applications.Any())
                return;

            // Get an Azure Table Storage client
            var tableClient = await GetTableClient();

            // Get a fresh access token for the Salesforce API
            await SalesForceApi.RefreshAccessToken(_logger);

            // Loop all new found applications
            foreach (var application in applications) {

                // Get linked job for application
                var job = await TeamTailorAPI.GetJob(application.Relationships.Job.Links.Related, _logger);
                // Load the internal ref nr, if the job has that tag
                TeamTailorAPI.LoadOpportunityRefNrFromTags(new () { job.Data });
                // Check if job was created by this integration (ie it has an internal ref nr)
                if (string.IsNullOrEmpty(job.Data?.Attributes?.SalesForceInternalRefId))
                    continue;

                var internalRef = job.Data?.Attributes?.SalesForceInternalRefId;
                // Try to get saved application
                var existingApplication = tableClient.Query<ApplicationTableEntity>(
                    e => e.InternalRefNr == internalRef && e.ApplicationId == application.Id
                ).FirstOrDefault();
                if (existingApplication != null) {
                    // This application has already been processed
                    _logger.LogInformation($"Application with id {application.Id} already exists");
                    continue;
                }

                // Add the application to table storage
                var newTableEntity = new ApplicationTableEntity() {
                    InternalRefNr = internalRef,
                    ApplicationId = application.Id
                };
                tableClient.AddEntity<ApplicationTableEntity>(newTableEntity);

                // Send application to Salesforce
            }

        }

        private async Task<TableClient> GetTableClient() {
            var tableClient = new TableClient(    
                new Uri(StorageAccountUri),
                StorageTableName,
                new TableSharedKeyCredential(StorageAccountName, StorageAccountKey));
            await tableClient.CreateIfNotExistsAsync();
            return tableClient;
        }
    }

    public class ApplicationTableEntity : ITableEntity {
        public string InternalRefNr { get; set; }
        public string ApplicationId { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
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
}
