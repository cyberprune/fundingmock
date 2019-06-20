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
        /// The amount of the profiled value, in pence.
        /// </summary>
        public long ProfiledValue { get; set; }

        /// <summary>
        /// The code for the period.
        /// </summary>
        public string PeriodCode { get; set; }
    }
}