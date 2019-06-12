using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FundingMock.Web.Enums
{
    /// <summary>
    /// Valid list of the different unique ways to identifier an organisation.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrganisationIdentifierType
    {
        /// <summary>
        /// UK Provider Reference Number - the unique identifier allocated to providers by the UK Register of Learning Providers (UKRLP) - 8 digits.
        /// </summary>
        UKPRN,

        /// <summary>
        /// The code of the local education authority.
        /// QUESTION - LA Code?
        /// </summary>
        LECode,

        /// <summary>
        ///  Unique provider identification number. A 6 digit number to represent a provider.
        /// </summary>
        UPIN,

        /// <summary>
        /// Unique Reference Number.
        /// </summary>
        URN,

        /// <summary>
        /// Unique Identifier (used for MATs).
        /// </summary>
        UID,

        /// <summary>
        /// The company number on Companies House.
        /// </summary>
        CompaniesHouseNumber,

        /// <summary>
        /// A code given to MATs.
        /// </summary>
        GroupID,

        /// <summary>
        /// ?
        /// </summary>
        RegionCode,

        /// <summary>
        /// The DfE number.
        /// </summary>
        DfeNumber
    }
}