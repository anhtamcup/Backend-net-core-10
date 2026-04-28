using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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
    }
}
