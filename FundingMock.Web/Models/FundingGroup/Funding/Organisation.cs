﻿using System.Collections.Generic;

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
        /// Organisation Details. This property is optional
        /// </summary>
        public OrganisationDetails OrganisationDetails { get; set; }
    }
}