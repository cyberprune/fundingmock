using System.Collections.Generic;

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
        /// Provider type (e.g. School, Academy, Special School) - not enumerated as this isn't controlled by CFS, but passed through from the Provider info (GIAS). #ThanksGav
        /// (Originally we had this as an enum - public enum ProviderType { School }
        /// </summary>
        public string ProviderType { get; set; }

        /// <summary>
        /// Identifier numbers for this organisation.
        /// </summary>
        public IEnumerable<OrganisationIdentifier> Identifiers { get; set; }
    }
}