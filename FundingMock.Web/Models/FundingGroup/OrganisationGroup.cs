using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FundingMock.Web.Enums;
using Newtonsoft.Json;

namespace FundingMock.Web.Models
{
    /// <summary>
    /// A grouping organistion (e.g. 'Camden', an LA) or (specific provider, 100023) or (country England)
    /// </summary>
    public class OrganisationGroup
    {
        /// <summary>
        /// The organisation group type. eg UKPRN or LACode
        /// </summary>
        [EnumDataType(typeof(OrganisationGroupTypeIdentifier))]
        [JsonProperty("mainIdentifierType")]
        public OrganisationGroupTypeIdentifier MainIdentifierType { get; set; }

        /// <summary>
        /// Value of the organisation type key, eg the actual UKPRN or LACode. 100023 or 202
        /// </summary>
        [JsonProperty("mainIdentifierCode")]
        public string MainIdentifierCode { get; set; }

        /// <summary>
        /// The name of the grouping organisation (e.g. in the case of the type being LA, this could be 'Camden').
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Text for Azure search to make this entity searchable. This is the name, but with punctuation etc removed to make it suitable for searching
        /// </summary>
        [JsonProperty("searchableName")]
        public string SearchableName { get; set; }

        /// <summary>
        /// Identifier numbers for this organisation.
        /// </summary>
        [JsonProperty("identifiers")]
        public IEnumerable<OrganisationIdentifier> Identifiers { get; set; }
    }
}