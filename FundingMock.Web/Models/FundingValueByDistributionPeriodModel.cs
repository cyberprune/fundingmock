namespace FundingMock.Web.Models
{
    public class FundingValueByDistributionPeriodModel
    {
        /// <summary>
        /// QUESTION: Should this be DistributionPeriodCode for consistency?
        /// </summary>
        public string DistributionPeriod { get; set; }

        /// <summary>
        /// Conflicts with class name
        /// </summary>
        public int FundingValueByDistributionPeriod { get; set; }

        public FundingLine FundingLines { get; set; }
    }
}