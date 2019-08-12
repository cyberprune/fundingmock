using Newtonsoft.Json;

namespace FundingMock.Web.Models
{
    /// <summary>
    /// Limited information about a provider.
    /// </summary>
    public class ProviderInformationModel
    {
        /// <summary>
        /// The UKPRN of the organisation/provider.
        /// </summary>
        [JsonProperty("ukPrn")]
        public string Ukprn { get; set; }

        /// <summary>
        /// The id of the organisation in provider api.
        /// </summary>
        [JsonProperty("providerVersionId")]
        public string ProviderVersionId { get; set; }
    }
}