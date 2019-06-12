namespace FundingMock.Web.Models
{
    /// <summary>
    /// A funding item.
    /// </summary>
    public class Funding
    {
        /// <summary>
        /// A unique id for this funding. In format 'schema:v{schemaVersion}/{stream.Code}/template:v{templateVersion}/{groupingOrg.Name}/{period.Code}/funding:v{fundingVersion}/{organisation.Name}'.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The version of this funding (i.e. for this provider).
        /// </summary>
        public int FundingVersion { get; set; }

        /// <summary>
        /// QUESTION - is this always provider at this level? Should we rename it?
        /// </summary>
        public Organisation Organisation { get; set; }

        /// <summary>
        /// The funding stream the funding relates to.
        /// QUESTION - At this level should we just have stream code for brevity?
        /// </summary>
        public Stream Stream { get; set; }

        /// <summary>
        /// The funding period the funding relates to.
        /// QUESTION - At this level should we just have period code for brevity?
        /// </summary>
        public Period Period { get; set; }

        /// <summary>
        /// Funding value.
        /// </summary>
        public FundingValue FundingValue { get; set; }
    }
}