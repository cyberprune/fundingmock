using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FundingMock.Web.Models
{
    /// <summary>
    /// A period that a funding line covers.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum FundingLinePeriodType
    {
        /// <summary>
        /// A Calender month.
        /// </summary>
        CalendarMonth,
    }
}