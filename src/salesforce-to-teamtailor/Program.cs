using Azure.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Credential retrieval strategy based on build configuration to speed
// up authentiation process.
var azureCredentials = new DefaultAzureCredential(new DefaultAzureCredentialOptions
{
    // Exclude these for all build configurations
    ExcludeEnvironmentCredential = true,
    ExcludeWorkloadIdentityCredential = true,

#if DEBUG
    // Development exclusion overrides
    ExcludeManagedIdentityCredential = true

#else
    // Production exclusion overrides
    ExcludeAzureCliCredential = true,
    ExcludeAzureDeveloperCliCredential = true,
    ExcludeAzurePowerShellCredential = true,
    ExcludeVisualStudioCredential = true
#endif
});

var builder = FunctionsApplication.CreateBuilder(args);

builder.Configuration.AddAzureKeyVault(
    new Uri($"https://kv-sf-to-tt-{Environment.GetEnvironmentVariable("APP_ENVIRONMENT") ?? "dev"}.vault.azure.net/"),
    azureCredentials
);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
