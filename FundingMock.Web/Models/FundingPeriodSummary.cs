using System;

namespace FundingMock.Web.Models
{
    public class FundingPeriodSummary
    {
        public string FundingPeriodCode { get; set; }

        public string FundingPeriodName { get; set; }

        public DateTimeOffset StartDate { get; set; }

        public DateTimeOffset EndDate { get; set; }
    }
}