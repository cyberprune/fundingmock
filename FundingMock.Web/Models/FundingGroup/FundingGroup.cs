using System;
using System.ComponentModel.DataAnnotations;

namespace FundingMock.Web.Models
{
    /// <summary>
    /// A funding group (a parent grouping organisation - such as an LA, MAT, Region etc...).
    /// </summary>
    public abstract class FundingGroup
    {
        /// <summary>
        /// Unique identifier of this funding group / business event (in format $"schema:v{schemaVersion}/{stream.Code}/template:v{templateVersion}/{groupingOrg.Name}/{period.Code}/funding:v{fundingVersion}").
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Version number of the published data. If there are changes to the funding for this organisation in this period, this number would increase.
        /// </summary>
        public string FundingVersion { get; set; }

        /// <summary>
        /// The funding status (i.e. published).
        /// </summary>
        [EnumDataType(typeof(FundingStatus))]
        public FundingStatus Status { get; set; }

        /// <summary>
        /// The funding stream the funding relates to.
        /// </summary>
        public StreamWithTemplateVersion FundingStream { get; set; }

        /// <summary>
        /// The funding period the funding relates to.
        /// </summary>
        public Period Period { get; set; }

        /// <summary>
        /// The grouped organisation or region (e.g. if we are grouping by LA, the organisation may be Camden).
        /// </summary>
        public GroupingOrganisation GroupedBy { get; set; }

        /// <summary>
        /// Funding value breakdown
        /// </summary>
        public FundingValue FundingValue { get; set; }

        /// <summary>
        /// Does the grouping reflect how the money is paid ('Payment') or is it just useful to show it this way? ('Informational'). 
        /// </summary>
        [EnumDataType(typeof(GroupingReason))]
        public GroupingReason GroupingReason { get; set; }

        /// <summary>
        /// The date the funding was published by a business user.
        /// </summary>
        public DateTimeOffset StatusChangedDate { get; set; }

        /// <summary>
        /// Date and time when the allocation can be published externally.
        /// </summary>
        public DateTimeOffset ExternalPublicationDate { get; set; }

        /// <summary>
        /// The date the payment will be made to the provider.
        /// </summary>
        public DateTimeOffset? PaymentDate { get; set; }
    }
}