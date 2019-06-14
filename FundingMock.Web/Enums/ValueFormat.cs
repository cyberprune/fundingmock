using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FundingMock.Web.Enums
{
    /// <summary>
    /// Valid list of the ways a number show be displayed
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ValueFormat
    {
        /// <summary>
        /// A number (e.g. a pupil number).
        /// </summary>
        Number,

        /// <summary>
        /// A percentage amount.
        /// </summary>
        Percentage,

        /// <summary>
        /// A currency.
        /// </summary>
        Currency,
    }
}