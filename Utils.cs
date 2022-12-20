using System.Text.Json;

namespace Magello {

    public static class Utils {
        public static JsonSerializerOptions GetJsonSerializer() {
            return new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
            };
        }

        public static string CreateUrl(
            string host,
            string endpoint, 
            Dictionary<string, string>? query) 
        {
            var uri = new UriBuilder();
            uri.Scheme = "https";
            uri.Host = host;
            if (!endpoint.StartsWith("/"))
                endpoint = $"/{endpoint}";
            uri.Path = endpoint;
            if (query != null) {
                uri.Query = string.Join(
                    "&",
                    query.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={kvp.Value}")
                );
            }
            return uri.Uri.AbsoluteUri;
        }

    }


}