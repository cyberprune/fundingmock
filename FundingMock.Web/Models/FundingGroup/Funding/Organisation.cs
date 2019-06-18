using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FundingMock.Web.Models.Providers;

namespace FundingMock.Web.Models
{
    /// <summary>
    /// An organisation.
    /// </summary>
    public class Organisation
    {
        /// <summary>
        /// The name of the organisation.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Text for Azure search to make this entity searchable. This is the name, but with punctuation etc removed to make it suitable for searching
        /// </summary>
        public string SearchableName { get; set; }

        /// <summary>
        /// Identifier numbers for this organisation.
        /// </summary>
        public IEnumerable<OrganisationIdentifier> Identifiers { get; set; }

        public string ProviderVersionId { get; set; }

        /// <summary>
        /// Provider type (e.g. School, Academy, Special School) - not enumerated as this isn't controlled by CFS, but passed through from the Provider info (GIAS)
        /// </summary>
        public string ProviderType { get; set; }

        /// <summary>
        /// TODO: Find out if this is required in the logical model
        /// </summary>
        public string ProviderSubType { get; set; }

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
        /// TODO: Find out if this is required in the logical model - used for variations
        /// </summary>
        public string ReasonEstablishmentOpened { get; set; }

        /// <summary>
        /// TODO: Find out if this is required in the logical model - used for variations
        /// </summary>
        public string ReasonEstablishmentClosed { get; set; }

        /// <summary>
        /// TODO: Find out if this is required in the logical model - used for variations
        /// </summary>
        public string Successor { get; set; }

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