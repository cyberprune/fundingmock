using System;
using System.ComponentModel.DataAnnotations;
using FundingMock.Web.Enums;
using FundingMock.Web.Models.Providers;

namespace FundingMock.Web.Models
{
    public class OrganisationDetails
    {
        /// <summary>
        /// Date Opened
        /// </summary>
        public DateTimeOffset? DateOpened { get; set; }

        /// <summary>
        /// Date Closed
        /// </summary>
        public DateTimeOffset? DateClosed { get; set; }

        public string Status { get; set; }

        /// <summary>
        /// TODO: Find out if this is required in the logical model
        /// </summary>
        public string PhaseOfEducation { get; set; }

        /// <summary>
        /// Optional open reason from the list of GIAS Open Reasons
        /// </summary>
        [EnumDataType(typeof(ProviderOpenReason))]
        public ProviderOpenReason? OpenReason { get; set; }

        /// <summary>
        /// Optional close reason from list of GIAS Close Reasons
        /// </summary>
        [EnumDataType(typeof(ProviderCloseReason))]
        public ProviderCloseReason? CloseReason { get; set; }

        /// <summary>
        /// TODO: Find out if this is required in the logical model
        /// </summary>
        [EnumDataType(typeof(TrustStatus))]
        public TrustStatus TrustStatus { get; set; }

        /// <summary>
        /// TODO: Find out if this is required in the logical model
        /// </summary>
        public string TrustName { get; set; }
    }
}