using FundingMock.Web.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace FundingMock.Web.Models
{
    /// <summary>
    /// Details about the period.
    /// </summary>
    public class Period
    {
        /// <summary>
        /// The code for the period (e.g. AY1920).
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// The name of the period (e.g. Academic Year 2019-20). 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The type of the period (academic or financial year).
        /// </summary>
        [EnumDataType(typeof(PeriodType))]
        public PeriodType Type { get; set; }

        /// <summary>
        /// The start date for the period.
        /// </summary>
        public DateTimeOffset StartDate { get; set; }

        /// <summary>
        /// The end date for the period.
        public DateTimeOffset EndDate { get; set; }
    }
}