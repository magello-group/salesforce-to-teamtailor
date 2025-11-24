using System.Diagnostics;
using Microsoft.Extensions.Configuration;

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
    }

}
