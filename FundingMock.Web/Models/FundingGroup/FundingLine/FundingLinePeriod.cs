using System.ComponentModel.DataAnnotations;

namespace FundingMock.Web.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class FundingLinePeriod
    {
        /// <summary>
        /// 
        /// </summary>
        [EnumDataType(typeof(FundingLinePeriodType))]
        public FundingLinePeriodType Type { get; set; }

        /// <summary>
        /// (e.g. if type is 'Calendar Month', this could be 'April').
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Which year is the period in.
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Which occurance this month (note that this is 1 indexed).
        /// </summary>
        public int Occurence { get; set; }

        /// <summary>
        /// The amount of the profiling value, in pence.
        /// </summary>
        public int ProfilingValue { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Period Period { get; set; }
    }
}