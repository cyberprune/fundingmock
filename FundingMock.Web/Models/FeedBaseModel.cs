using Newtonsoft.Json;

namespace FundingMock.Web.Models
{
    /// <summary>
    /// Base model for the feed method.
    /// </summary>
    public class FeedBaseModel
    {
        /// <summary>
        /// Schema URI to validate against.
        /// </summary>
        [JsonProperty(PropertyName = "$schema")]
        public string SchemaUri { get; set; }

        /// <summary>
        /// The schema version. Schema here refers to the Classes etc.., not the specific model being used.
        /// </summary>
        public string SchemaVersion { get; set; }

        /// <summary>
        /// The funding group (a parent grouping organisation - such as an LA, MAT, Region etc...).
        /// </summary>
        public FundingGroup FundingGroup { get; set; }
    }
}