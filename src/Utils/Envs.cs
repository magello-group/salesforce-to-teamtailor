using System.Diagnostics;

namespace Magello {

    public static class Envs {

        public static readonly string E_TeamTailorApiToken = "TEAMTAILOR_API_TOKEN";
        public static readonly string E_SalesForceApiHost = "SALESFORCE_API_HOST";
        public static readonly string E_SalesForceApiTokenEndpoint = "SALESFORCE_TOKEN_ENDPOINT";
        public static readonly string E_SalesForceApiClientKey = "SALESFORCE_CLIENT_KEY";
        public static readonly string E_SalesForceApiClientSecret = "SALESFORCE_CLIENT_SECRET";
        public static readonly string E_AzStorageAccountUri = "TABLE_STORAGE_URI";
        public static readonly string E_AzStorageAccountName = "TABLE_STORAGE_NAME";
        public static readonly string E_AzStorageAccountKey = "TABLE_STORAGE_KEY";
        public static readonly string E_SalesForceCustomFieldId = "SFID_CUSTOM_FIELD_ID";
        public static readonly string E_TeamTailorBaseUrl = "TEAMTAILOR_BASE_URL";
        
        public static string? GetEnvVar(string envVarName) {
            return Environment.GetEnvironmentVariable(envVarName) ?? null;
        }

        private static bool EnvVarIsSet(string envVarName) {
            return !string.IsNullOrWhiteSpace(GetEnvVar(envVarName));
        }
        
        // Pre-flight env checks
        public static void PreFlightEnvChecks() {
            var envs = new List<string> {
                E_TeamTailorApiToken, 
                E_SalesForceApiHost, 
                E_SalesForceApiTokenEndpoint, 
                E_SalesForceApiClientKey, 
                E_SalesForceApiClientSecret, 
                E_AzStorageAccountUri, 
                E_AzStorageAccountName, 
                E_AzStorageAccountKey, 
                E_SalesForceCustomFieldId,
                E_TeamTailorBaseUrl,
            };
            envs.ForEach(e => 
                Debug.Assert(EnvVarIsSet(e), $"Environment variable {e} should not be empty")
            );
        }

    }

}
