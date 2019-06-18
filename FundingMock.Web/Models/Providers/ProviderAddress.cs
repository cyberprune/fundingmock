using Newtonsoft.Json;

namespace FundingMock.Web.Models.Providers
{
    public class ProviderAddress
    {
        [JsonProperty("town")]
        public string Town { get; set; }

        [JsonProperty("postcode")]
        public string Postcode { get; set; }
    }
}
