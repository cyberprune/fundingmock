using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FundingMock.Web.Enums
{
    /// <summary>
    /// 
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AggregationType
    {
        /// <summary>
        /// 
        /// </summary>
        None,

        /// <summary>
        /// 
        /// </summary>
        Average,

        /// <summary>
        /// 
        /// </summary>
        Sum
    }
}