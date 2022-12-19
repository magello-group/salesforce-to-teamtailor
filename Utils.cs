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

    }


}