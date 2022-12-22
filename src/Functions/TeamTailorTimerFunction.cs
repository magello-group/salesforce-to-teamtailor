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
            _logger.LogInformation($"TeamTailorTimerFunction executed at: {DateTime.Now}");
            _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus?.Next}");

            // Get the datetime of our last run
            var lastRun = DateTime.Now;
            if (myTimer.ScheduleStatus?.Last != null)
                lastRun = myTimer.ScheduleStatus.Last;

            _logger.LogInformation($"Last run was at {lastRun.ToLongDateString()}");

            // Get applications created since last run
            var applications = await TeamTailorAPI.GetApplications(lastRun, _logger);
            _logger.LogInformation($"Found {applications.Count} applications since last run");

            if (!applications.Any())
                return;

            // Get a fresh access token
            await SalesForceApi.RefreshAccessToken(_logger);

            // Loop all new found applications
            foreach (var application in applications) {
                var job = await TeamTailorAPI.GetJob(application.Relationships.Job.Links.Related, _logger);
                // Load (if it exists) the internal ref nr
                TeamTailorAPI.LoadOpportunityRefNrFromTags(new () { job.Data });
                if (string.IsNullOrEmpty(job.Data?.Attributes?.SalesForceInternalRefId))
                    // This is not a job created via the integration
                    continue;
                // Add the application to the salesforce opportunity matching the internal refnr

            }

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
