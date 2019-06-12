namespace FundingMock.Web.Models
{
    /// <summary>
    /// Base model for the feed method.
    /// </summary>
    public class FeedBaseModel
    {
        /// <summary>
        /// The schema version. Schema here refers to the Classes etc.., not the specific model being used.
        /// </summary>
        public int SchemaVersion { get; set; }

        /// <summary>
        /// The funding group (a parent grouping organisation - such as an LA, MAT, Region etc...).
        /// </summary>
        public FundingGroup FundingGroup { get; set; }
    }
}