using System.ComponentModel.DataAnnotations;
using FundingMock.Web.Enums;
using Newtonsoft.Json;

namespace FundingMock.Web.Models
{
    /// <summary>
    /// A key/vaue pairing representing a organisation identifier.
    /// </summary>
    public class OrganisationIdentifier
    {
        /// <summary>
        /// The type of orgranisation identifier (e.g. UKPRN). 
        /// </summary>
        [EnumDataType(typeof(OrganisationTypeIdentifier))]
        [JsonProperty("type")]
        public OrganisationTypeIdentifier Type { get; set; }

        /// <summary>
        /// The value of this identifier type (e.g. if the type is UKPRN, then the value may be 12345678. 
        /// If the type is LECode, the value may be 'LA 203').
        /// </summary>
        [JsonProperty("value")]
        public string Value { get; set; }
    }
}