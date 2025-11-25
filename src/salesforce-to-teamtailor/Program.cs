using System.Configuration;
using System.Net.Http.Headers;
using KISS.HttpClientAuthentication;
using Magello.SalesforceToTeamtailor.Api;
using Magello.SalesforceToTeamtailor.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.Configuration.AddAzureKeyVault(
    new Uri($"https://kv-sf-to-tt-{Environment.GetEnvironmentVariable("APP_ENVIRONMENT") ?? "dev"}.vault.azure.net/"),
    Utils.AzureCredentials
);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Services
    .AddHttpClient<ITeamTailorApi, TeamTailorAPI>((services, client) =>
    {
        var configuration = services.GetRequiredService<IConfiguration>();

        var teamTailorApiToken = configuration.GetValue<string>(Envs.E_TeamTailorApiToken) ??
            throw new InvalidOperationException($"{Envs.E_TeamTailorApiToken} not set in configuration");

        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Add("Accept", "application/vnd.api+json");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", $"token={teamTailorApiToken}");
        client.DefaultRequestHeaders.Add("X-Api-Version", "20210218");

    });

builder.Services
    .AddHttpClient<ISalesForceApi, SalesForceApi>((services, client) =>
    {
        var configuration = services.GetRequiredService<IConfiguration>();
        var salesForceApiHost = configuration.GetValue<string>($"{nameof(SalesForceApi)}:Url") ??
            throw new InvalidOperationException($"{nameof(SalesForceApi)}:Url not set in configuration");
        client.BaseAddress = new Uri(salesForceApiHost);
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Add("Accept", "application/json");
    })
    .AddAuthenticatedHttpMessageHandler(nameof(SalesForceApi));

builder.Build().Run();
