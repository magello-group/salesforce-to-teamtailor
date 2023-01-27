# salesforce-to-teamtailor

Azure function middleware to handle API-to-API communication between SalesForce and TeamTailor

# To redesign the Teamtailor job body

1) Open `src/templates/teamtailor-body.scriban-html`
2) Use fields mapped in `utils/Utils.cs` function `TemplateTeamTailorBody(..)`
3) The template is templated using `scriban` and Teamtailor jobs accept `HTML` as body
4) Redeploy the function (function is `magellosalesforceteamtailor` on Azure for publish profile download)

# Execution flow

Salesforce UI button -> 
    starts flow -> 
    calls invocation class -> 
    calls api-class -> 
    fetches metadata + settings ->
    calls az function

azure function (salesforce -> teamtailor):
    
* deserializes data
* maps sf deserialized object to JsonNode
    * templates tt job body from templates/teamtailor-body.scriban-html
* creates teamtailor job
* adds custom field values with salesforce id to job

azure function (teamtailor -> salesforce)

* polls teamtailor applications since last run
* if no new = exit
* fetches custom field definitions
* refreshes salesforce access token
* loops applications:
    * get job for application
    * get custom field values for job
    * if no salesforce id found in field values = exit (not a salesforce job)
    * check if application already processed
    * if yes = exit
    * get candidate from application
    * add application to table storage as processed
    * create case in salesforce

# Salesforce modifications

* Flow
    * Low-code tool for process generation
* Invocation-class
    * Class with custom @annotations for flow-usage (input, output..)
* API-class
    * Code that does the actual call to the API middleware
* Metadata
    * Table with mappings between logins in SF and userids in TT
* API user
    * User with API only access, with client id and secret
* Connected app
    * OAuth enabled connected app (with run as above api user)
* Settings
    * String value that holds the url and token key for the Azure Function running the middleware
* Object fields
    * Opportunity
        * Sent to teamtailor = bool
        * Teamtailor job link = url
    * Case
        * Teamtailor application link = url
* UI changes
    * TeamTailor-button
    * Fields (see above)

# TeamTailor modifications

* Custom fields:
    * salesforceid (API only)

# Required configuration

* Teamtailor: Create custom field and fetch the id (see how under env var documentation below)
* Salesforce: Add salesforce user -> teamtailor userid mapping to metadata
* Salesforce: Add API-only user + profile with rights to create cases
* Salesforce: Add connected app with client credential flow and set "run as" to above API-user

# Environment variables

| Name | Description |
| --- | --- |
| TEAMTAILOR_API_TOKEN | API token generated from TeamTailor |
| SALESFORCE_CLIENT_KEY | The OAuth client key for the generated API user in Salesforce |
| SALESFORCE_CLIENT_SECRET | The OAuth client secret for the generated API user in Salesforce |
| SALESFORCE_TOKEN_ENDPOINT | Most likely /services/oauth2/token |
| SALESFORCE_API_HOST | E.g. r2m--dev1.sandbox.my.salesforce.com |
| TABLE_STORAGE_URI | URI to the Azure Table Storage used for duplication checks, e.g. https://example.table.core.windows.net/ |
| TABLE_STORAGE_KEY | Key for above storage |
| TABLE_STORAGE_NAME | Name for above storage |
| SFID_CUSTOM_FIELD_ID | ID of the custom field used for the salesforceid. This can be obtained through an API call to: https://api.teamtailor.com/v1/custom-fields | 
| TEAMTAILOR_BASE_URL | Base url to the company on teamtailor, e.g. https://app.teamtailor.com/companies/ABCD-ABCab12 |

# TODO

* Import changeset to production
* User user "Insights Integration" - modify profile
* Check if possible to change domain from r2m to magello
* bit.ly url used in custom settings because of length limit of 100 (is 250 in sandbox)