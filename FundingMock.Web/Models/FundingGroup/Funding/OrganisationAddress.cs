using Newtonsoft.Json;

namespace FundingMock.Web.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class OrganisationAddress
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("town")]
        public string Town { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("postcode")]
        public string Postcode { get; set; }
    }
}