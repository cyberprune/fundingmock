using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FundingMock.Web.Models
{
    /// <summary>
    /// The funding status (wehter its released or otherwise).
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum FundingStatus
    {
        /// <summary>
        /// Approved.
        /// </summary>
        Approved,

        /// <summary>
        /// Updated.
        /// </summary>
        Updated,

        /// <summary>
        /// Released - previously published.
        /// </summary>
        Released,
    }
}