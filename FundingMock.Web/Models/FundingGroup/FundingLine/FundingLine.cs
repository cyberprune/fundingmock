using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FundingMock.Web.Models
{
    /// <summary>
    /// A funding line.
    /// </summary>
    public class FundingLine
    {
        public FundingLine()
        {
            Type = FundingLineType.Information;
        }

        /// <summary>
        /// The name of a funding line (e.g. "Total funding line").
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Funding Line Code - unique code within the template to lookup this specific funding line.
        /// Used to map this funding line in consuming systems (eg nav for payment)
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string FundingLineCode { get; set; }

        /// <summary>
        /// The funding value in pence.
        /// </summary>
        public long Value { get; set; }

        /// <summary>
        /// A unique ID (in terms of template, not data) for this funding line (e.g. 345).
        /// </summary>
        public uint TemplateLineId { get; set; }

        /// <summary>
        /// The type of the funding line (e.g. paid on this basis, or informational only).
        /// </summary>
        [EnumDataType(typeof(FundingLineType))]
        public FundingLineType Type { get; set; }

        /// <summary>
        /// The periods that this funding line where paid in / are due to be paid in.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<FundingLinePeriod> ProfilePeriods { get; set; }

        /// <summary>
        /// Calculations that make up this funding line.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<Calculation> Calculations { get; set; }


        /// <summary>
        /// Sub funding lines that make up this funding line.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<FundingLine> FundingLines { get; set; }
    }
}