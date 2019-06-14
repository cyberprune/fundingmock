using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FundingMock.Web.Models
{
    /// <summary>
    /// The funding line type (actual payment or informational only).
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum FundingLineType
    {
        /// <summary>
        /// An actual payment.
        /// </summary>
        Payment,

        /// <summary>
        /// ,
        /// </summary>
        Information,
    }
}