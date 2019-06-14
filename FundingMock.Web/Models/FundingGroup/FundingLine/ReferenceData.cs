using FundingMock.Web.Enums;
using System.ComponentModel.DataAnnotations;

namespace FundingMock.Web.Models
{
    /// <summary>
    /// Data that is required by calculations.
    /// </summary>
    public class ReferenceData
    {
        /// <summary>
        /// The name of this reference data (e.g. 'Academic year 2018 to 2019 pupil number on roll').
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The format of the reference data value (e.g. Percentage).
        /// </summary>
        [EnumDataType(typeof(ValueFormat))]
        public ValueFormat Format { get; set; }

        /// <summary>
        /// The reference data value.
        /// </summary>
        public string Value { get; set; }
    }
}