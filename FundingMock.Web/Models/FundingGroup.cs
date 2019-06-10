using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FundingMock.Web.Models
{
    public class FundingGroup
    {
        public FundingStreamSummary FundingStream { get; set; }

        public FundingPeriodSummary FundingPeriod { get; set; }

        [EnumDataType(typeof(FundingStatus))]
        public FundingStatus FundingStatus { get; set; }

        public DateTimeOffset FundingStatusDate { get; set; }

        public GroupingOrganisation GroupingOrganisation { get; set; }

        [EnumDataType(typeof(GroupingReason))]
        public GroupingReason GroupingReason { get; set; }

        /// <summary>
        /// Eg the version of the template of PE and sport
        /// </summary>
        public int TemplateVersion { get; set; }

        /// <summary>
        /// Version number of the published data
        /// </summary>
        public string FundingVersion { get; set; }

        /// <summary>
        /// Unique identifier of this business event
        /// </summary>
        public string FundingIdentifier { get; set; }

        /// <summary>
        /// Date and time when the allocation can be published externally
        /// </summary>
        public DateTimeOffset? ExternalPublicationDate { get; set; }

        /// <summary>
        /// The date the payment will be made to the provider
        /// </summary>
        public DateTimeOffset? PaymentDate { get; set; }

        public FundingValue FundingValue { get; set; }

        public IEnumerable<Fundings> Fundings { get; set; }

    }
}
