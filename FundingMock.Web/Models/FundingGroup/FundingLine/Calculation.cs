using FundingMock.Web.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FundingMock.Web.Models
{
    /// <summary>
    /// A calculation used to build up a funding line.
    /// </summary>
    public class Calculation
    {
        /// <summary>
        /// The name of the calculation.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The template calculation id (i.e. a way to get to this property in the template).
        /// </summary>
        public int TemplateCalculationId { get; set; }

        /// <summary>
        /// The value the calculation is resulting in.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The way the value should show.
        /// </summary>
        [EnumDataType(typeof(ValueFormat))]
        public ValueFormat ValueFormat { get; set; }

        /// <summary>
        /// The type of calculation.
        /// </summary>
        [EnumDataType(typeof(CalculationType))]
        public CalculationType Type { get; set; }

        /// <summary>
        /// Presentation data about how a formula is built up.
        /// </summary>
        public string FormulaText { get; set; }

        /// <summary>
        /// Sub level calculations.
        /// </summary>
        public IEnumerable<Calculation> Calculations { get; set; }

        /// <summary>
        /// Reference data this these calculations depend on.
        /// </summary>
        public IEnumerable<ReferenceData> ReferenceData { get; set; }
    }
}