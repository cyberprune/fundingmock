namespace FundingMock.Web.Models
{
    /// <summary>
    /// A funding item.
    /// </summary>
    public class ProviderFunding
    {
        /// <summary>
        /// A unique id for this funding. In format 'schema:v{schemaVersion}/{stream.Code}/template:v{templateVersion}/{groupingOrg.Name}/{period.Code}/funding:v{fundingVersion}/{organisation.Name}'.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The organisation for which the funding is for.
        /// </summary>
        public Organisation Organisation { get; set; }

        public Variations Variations { get; set; }

        /// <summary>
        /// The funding stream the funding relates to.
        /// </summary>
        public string StreamCode { get; set; }

        /// <summary>
        /// The funding period the funding relates to.
        /// </summary>
        public string PeriodCode { get; set; }

        /// <summary>
        /// Funding value.
        /// </summary>
        public FundingValue FundingValue { get; set; }

    }
}