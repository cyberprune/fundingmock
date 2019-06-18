﻿using Newtonsoft.Json;
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
        /// A number (e.g. a pupil number). eg 5, the return value should be 5 and 2.7334 should return 2.7334
        /// </summary>
        Number,

        /// <summary>
        /// A percentage amount. eg for 25%, the return value should be 25
        /// </summary>
        Percentage,

        /// <summary>
        /// A currency. for example for £5.83, the return value should be 583
        /// </summary>
        Currency,
    }
}