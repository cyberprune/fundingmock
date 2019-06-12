using System.Collections.Generic;

namespace FundingMock.Web.Models
{
    /// <summary>
    /// Funding values grouped by the distribution period (envelope) they are paid in.
    /// </summary>
    public class FundingValueByDistributionPeriod : Period
    {
        /// <summary>
        /// The overall value for the distribution period.
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// The lines that make up this funding. 
        /// </summary>
        public IEnumerable<FundingLine> FundingLines { get; set; }
    }
}