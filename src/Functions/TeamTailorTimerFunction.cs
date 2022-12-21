using System;
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
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus?.Next}");

            // Get the datetime of our last run
            var lastRun = DateTime.Now;
            if (myTimer.ScheduleStatus?.Last != null)
                lastRun = myTimer.ScheduleStatus.Last;

            // Get all jobs with the "salesforce" tag
            //var taggedJobs = await TeamTailorAPI.GetTaggedJobs(_logger);
            // Get applications created since last run
            var applications = await TeamTailorAPI.GetApplications(lastRun, _logger);

            if (applications == null)
                return;

            _logger.LogInformation($"Found {applications.Count} applications since last run");

            foreach (var application in applications) {
                var job = await TeamTailorAPI.GetJob(application.Relationships.Job.Links.Related, _logger);
            }

            var serviceClient = new TableServiceClient(
                new Uri(StorageAccountUri),
                new TableSharedKeyCredential(StorageAccountName, StorageAccountKey));

            // Get or create our applications table
            var queryTableResults = serviceClient.Query(filter: $"TableName eq '{StorageTableName}'");
            TableItem table;
            if (queryTableResults.Count() == 0)
                table = serviceClient.CreateTableIfNotExists(StorageTableName);
            else
                table = queryTableResults.First();

            // Create a client for the applications table
            var tableClient = new TableClient(
                new Uri(StorageAccountUri),
                StorageTableName,
                new TableSharedKeyCredential(StorageAccountName, StorageAccountKey));

            /*foreach (var job in taggedJobs) {
                var refId = job.Attributes.SalesForceInternalRefId;
                var existingApplications = tableClient.Query<TableEntity>(filter: $"PartitionKey eq '{refId}'");

            }*/
            

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
}
