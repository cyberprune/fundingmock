using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace FundingMock.Web.Models
{
    /// <summary>
    /// Top level object for an atom feed.
    /// </summary>
    public class FeedResponseModel
    {
        /// <summary>
        /// Id of the feed (not used in our case).
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Feed title.
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// Author of the feed.
        /// </summary>
        [JsonProperty("author")]
        public FeedResponseAuthor Author { get; set; }

        /// <summary>
        /// When the feed was updated (newest updated date of any funding).
        /// </summary>
        [JsonProperty("updated")]
        public DateTime Updated { get; set; }

        /// <summary>
        /// Copyright information.
        /// </summary>
        [JsonProperty("rights")]
        public string Rights { get; set; }

        /// <summary>
        /// Array of relational links.
        /// </summary>
        [JsonProperty("link")]
        public List<FeedLink> Link { get; set; }

        /// <summary>
        /// Array of entires that ultimately contain fundings.
        /// </summary>
        [JsonProperty("atomEntry")]
        public FeedResponseContentModel[] AtomEntry { get; set; }
    }
}