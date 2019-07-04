using Newtonsoft.Json;

namespace FundingMock.Web.Models
{
    /// <summary>
    /// Information about the author of the feed.
    /// </summary>
    public class FeedResponseAuthor
    {
        /// <summary>
        /// Email address of the author.
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// Name of the author.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}