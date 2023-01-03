# salesforce-to-teamtailor
Azure function middleware to handle API-to-API communication between SalesForce and TeamTailor

# Salesforce
* Flow
    * Low-code tool for process generation
* Invocation-class
    * Class with custom @annotations for flow-usage (input, output..)
* API-class
    * Code that does the actual call to the API middleware
* Metadata
    * Table with mappings between logins in SF and userids in TT
* Settings
    * String value that holds the url and token key for the Azure Function running the middleware

Button -> Flow -> Invocation -> API-class -> Metadata + Settings -> Call API

# Middleware
* JSON -> POJO mapping
* POJO -> TT/SF field mapping
* TeamTailor body rendering
* TeamTailor API token
* Salesforce API token
* Business logic

# TeamTailor
* Anpassade fält för jobb:
    * salesforceid (API only)
    * salesforcerefid (API only)

# TODO
* Import changeset to production
* Create connected app with client credential flow
* Configure Azure Function with client-id/secret for connected app
* Create user + profile with "Api Only" permission set
* Add created user as "Run as" for connected app
* Document ENVs
* Improve documentation for upkeep