using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FundingMock.Web.Enums
{
    /// <summary>
    /// Valid list of calculation types.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CalculationType
    {
        /// <summary>
        /// A monetary amount.
        /// </summary>
        Cash,

        /// <summary>
        /// A monetary amount. 
        /// QUESTION - how is this different to cash?
        /// </summary>
        Rate,

        /// <summary>
        /// Number of pupils.
        /// </summary>
        PupilNumber,

        /// <summary>
        /// A number between 0 and 1.
        /// </summary>
        Weighting,

        /// <summary>
        /// ? QUESTION
        /// </summary>
        Scope,

        /// <summary>
        /// Informational information only.
        /// </summary>
        Information,

        /// <summary>
        /// ? QUESTION
        /// </summary>
        Drilldown,
    }
}