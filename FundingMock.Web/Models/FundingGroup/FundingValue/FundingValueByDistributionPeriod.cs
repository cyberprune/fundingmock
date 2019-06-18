using System.Collections.Generic;

namespace FundingMock.Web.Models
{
    /// <summary>
    /// Funding values grouped by the distribution period (envelope) they are paid in.
    /// </summary>
    public class FundingValueByDistributionPeriod
    {
        /// <summary>
        /// The overall value for the distribution period in pence. Rolled up from all child Funding Lines where Type = Payment
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// The lines that make up this funding. 
        /// </summary>
        public IEnumerable<FundingLine> FundingLines { get; set; }

        /// <summary>
        /// The funding period the funding relates to.
        /// </summary>
        public string DistributionPeriodCode { get; set; }
    }
}