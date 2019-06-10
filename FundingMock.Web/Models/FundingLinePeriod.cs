using System.ComponentModel.DataAnnotations;

namespace FundingMock.Web.Models
{
    public class FundingLinePeriod
    {
        public string FundingLinePeriodName { get; set; }

        public string FundingLinePeriodOccurence { get; set; }

        public int FundingLinePeriodProfilingValue { get; set; }

        [EnumDataType(typeof(FundingLinePeriodType))]
        public FundingLinePeriodType FundingLinePeriodType { get; set; }

        public int FundingLinePeriodYear { get; set; }

        public string FundingLinePeriodDistributionYear { get; set; }

    }
}