using System.Collections.Generic;

namespace FundingMock.Web.Models
{
    public class FundingValue
    {
        public int TotalFundingValue { get; set; }

        public IEnumerable<FundingValueByDistributionPeriodModel> FundingValueByDistributionPeriod { get; set; }
    }
}