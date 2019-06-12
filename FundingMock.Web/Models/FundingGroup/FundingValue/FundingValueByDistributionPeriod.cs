using System.Collections.Generic;

namespace FundingMock.Web.Models
{
    /// <summary>
    /// Funding values grouped by the distribution period (envelope) they are paid in.
    /// QUESTION - This used to inherit from Period, but took it away to make the JSON more concise.
    /// </summary>
    public class FundingValueByDistributionPeriod
    {
        /// <summary>
        /// The overall value for the distribution period.
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// The lines that make up this funding. 
        /// </summary>
        public IEnumerable<FundingLine> FundingLines { get; set; }

        /// <summary>
        /// The funding period the funding relates to.
        /// </summary>
        public string PeriodCode { get; set; }
    }
}