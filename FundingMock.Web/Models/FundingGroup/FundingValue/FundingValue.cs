﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace FundingMock.Web.Models
{
    /// <summary>
    /// The total amount paid, and the periods/envelopes that it was composed of.
    /// </summary>
    public class FundingValue
    {
        /// <summary>
        /// The funding value amount in pence. Rolled up from all child Funding Lines where Type = Payment
        /// </summary>
        [JsonProperty("totalValue")]
        public decimal TotalValue { get; set; }

        /// <summary>
        /// The lines that make up this funding. 
        /// </summary>
        [JsonProperty("fundingLines")]
        public IEnumerable<FundingLine> FundingLines { get; set; }

        /// <summary>
        /// An aggregate of distribution periods for this provider (aggregates from child Funding Lines)
        /// </summary>
        [JsonProperty("distributionPeriods")]
        public IEnumerable<FundingValueByDistributionPeriod> DistributionPeriods { get; set; }

        /// <summary>
        /// The periods that this funding line where paid in / are due to be paid in.
        /// </summary>
        [JsonProperty("profilePeriods", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<FundingLinePeriod> ProfilePeriods { get; set; }
    }
}