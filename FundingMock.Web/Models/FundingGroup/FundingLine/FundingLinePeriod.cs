using System.ComponentModel.DataAnnotations;

namespace FundingMock.Web.Models
{
    /// <summary>
    /// A funding line period (e.g. the 1st March payment in 2019), with relevant value data.
    /// </summary>
    public class FundingLinePeriod
    {
        /// <summary>
        /// The type of the period (e.g. CalendarMonth).
        /// </summary>
        [EnumDataType(typeof(FundingLinePeriodType))]
        public FundingLinePeriodType Type { get; set; }

        /// <summary>
        /// The value identifier for this period (e.g. if type is 'Calendar Month', this could be 'April').
        /// </summary>
        public string TypeValue { get; set; }

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
        /// Information about the period.
        /// QUESTION - should this be just PeriodCode at this level? It becomes quite heavy to have it here, and there is an API to look them up
        /// </summary>
        public Period Period { get; set; }
    }
}