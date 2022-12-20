using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Magello.SalesForceToTeamTailor
{
    public class TeamTailorJobPoll
    {
        private readonly ILogger _logger;

        public TeamTailorJobPoll(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<TeamTailorJobPoll>();
        }

        [Function("TeamTailorJobPoll")]
        public async Task Run([TimerTrigger("0 */5 * * * *", RunOnStartup = true)] MyInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");

            var taggedJobs = await TeamTailorAPI.GetTaggedJobs(_logger);
            _logger.LogInformation($"Found {taggedJobs?.Count} jobs");

            // Loop jobs
                // Check if job needs updating in SF
                // Send mail/slack (?)

        }
    }

    public class MyInfo
    {
        public MyScheduleStatus ScheduleStatus { get; set; }

        public bool IsPastDue { get; set; }
    }

    public class MyScheduleStatus
    {
        public DateTime Last { get; set; }

        public DateTime Next { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
