using Newtonsoft.Json;
using System;

namespace FundingMock.Web.Models
{
    /// <summary>
    /// 2nd level (atomEntry) object in an atom feed.
    /// </summary>
    public class FeedResponseContentModel
    {
        /// <summary>
        /// Id of the feed atom entry.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Title of the entry.
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// Author of the feed entry.
        /// </summary>
        [JsonProperty("author")]
        public FeedResponseAuthor Author { get; set; }

        /// <summary>
        /// When the feed entry was updated / created.
        /// </summary>
        [JsonProperty("updated")]
        public DateTime Updated { get; set; }

        /// <summary>
        /// Array containing relational links.
        /// </summary>
        [JsonProperty("link")]
        public FeedLink[] Link { get; set; }

        /// <summary>
        /// Content of the feed entry (the funding).
        /// </summary>
        [JsonProperty("content")]
        public FeedBaseModel Content { get; set; }
    }
}