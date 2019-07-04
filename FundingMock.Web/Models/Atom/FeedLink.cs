using Newtonsoft.Json;

namespace FundingMock.Web.Models
{
    /// <summary>
    /// Atom feeds link to previous, next, first etc...
    /// </summary>
    public class FeedLink
    {
        /// <summary>
        /// URI for the relational page.
        /// </summary>
        [JsonProperty("href")]
        public string Href { get; set; }

        /// <summary>
        /// What type of page (first, last, self, previous, last).
        /// </summary>
        [JsonProperty("rel")]
        public string Rel { get; set; }
    }
}