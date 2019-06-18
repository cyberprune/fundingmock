using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FundingMock.Web.Enums;

namespace FundingMock.Web.Models
{
    /// <summary>
    /// A grouping organistion (e.g. 'Camden', an LA).
    /// </summary>
    public class GroupedBy
    {
        /// <summary>
        /// The name of the grouping organisation (e.g. in the case of the type being LA, this could be 'Camden').
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The organisation group type.
        /// </summary>
        [EnumDataType(typeof(OrganisationType))]
        public OrganisationType Type { get; set; }

        /// <summary>
        /// Text for Azure search to make this entity searchable. This is the name, but with punctuation etc removed to make it suitable for searching
        /// </summary>
        public string SearchableName { get; set; }

        /// <summary>
        /// Identifier numbers for this organisation.
        /// </summary>
        public IEnumerable<OrganisationIdentifier> Identifiers { get; set; }
    }
}