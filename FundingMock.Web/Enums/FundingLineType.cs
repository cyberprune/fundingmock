using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FundingMock.Web.Models
{
    /// <summary>
    /// The funding line type (actual payment, Aggregate or informational only).
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum FundingLineType
    {
        /// <summary>
        /// An actual payment.
        /// </summary>
        Payment,

        /// <summary>
        /// QUESTION - is this different to aggregate?
        /// </summary>
        Information,

        /// <summary>
        /// QUESTION - is this different to information?
        /// </summary>
        Aggregate,
    }
}