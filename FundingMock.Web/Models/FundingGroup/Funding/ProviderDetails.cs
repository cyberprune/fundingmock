using System;
using System.ComponentModel.DataAnnotations;
using FundingMock.Web.Enums;
using Newtonsoft.Json;

namespace FundingMock.Web.Models
{
    /// <summary>
    /// (Optional) details about an provider. Passed through from the provider API.
    /// </summary>
    public class ProviderDetails
    {
        /// <summary>
        /// Date Opened.
        /// </summary>
        [JsonProperty("dateOpened")]
        public DateTimeOffset? DateOpened { get; set; }

        /// <summary>
        /// Date Closed.
        /// </summary>
        [JsonProperty("dateClosed")]
        public DateTimeOffset? DateClosed { get; set; }

        /// <summary>
        /// Status of the organisation (TODO find examples).
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; }

        /// <summary>
        /// TODO: Find out if this is required in the logical model
        /// </summary>
        [JsonProperty("phaseOfEducation")]
        public string PhaseOfEducation { get; set; }

        /// <summary>
        /// Optional open reason from the list of GIAS Open Reasons
        /// </summary>
        [EnumDataType(typeof(ProviderOpenReason))]
        [JsonProperty("openReason")]
        public ProviderOpenReason? OpenReason { get; set; }

        /// <summary>
        /// Optional close reason from list of GIAS Close Reasons
        /// </summary>
        [EnumDataType(typeof(ProviderCloseReason))]
        [JsonProperty("closeReason")]
        public ProviderCloseReason? CloseReason { get; set; }

        /// <summary>
        /// TODO: Find out if this is required in the logical model
        /// </summary>
        [EnumDataType(typeof(TrustStatus))]
        [JsonProperty("trustStatus")]
        public TrustStatus TrustStatus { get; set; }

        /// <summary>
        /// TODO: Find out if this is required in the logical model
        /// </summary>
        [JsonProperty("trustName")]
        public string TrustName { get; set; }

        /// <summary>
        /// Town
        /// </summary>
        [JsonProperty("town")]
        public string Town { get; set; }

        /// <summary>
        /// Postcode
        /// </summary>
        [JsonProperty("postcode")]
        public string Postcode { get; set; }
    }
}