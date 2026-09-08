using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text.Json;

namespace S3.Gateway.Common
{
    public static class Utility
    {
        public static string SerializeObjectLowerCase(object request)
        {
            var payload = JsonConvert.SerializeObject(
                request,
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    NullValueHandling = NullValueHandling.Ignore
                });

            return payload;
        }

        public static string GetJsonProperty(string rawBody, string property)
        {
            using var doc = JsonDocument.Parse(rawBody);
            var payloadRaw = doc.RootElement.GetProperty(property).GetRawText();
            return payloadRaw;
        }
    }
}
