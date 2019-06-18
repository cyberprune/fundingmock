using Newtonsoft.Json;

namespace FundingMock.Web.Models
{
    public class ProviderInformationModel
    {
        [JsonProperty("ukPrn")]
        public string Ukprn { get; set; }

        [JsonProperty("providerVersionId")]
        public string ProviderVersionId { get; set; }
    }
}