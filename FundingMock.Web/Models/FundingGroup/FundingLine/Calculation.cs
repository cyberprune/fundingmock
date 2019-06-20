using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FundingMock.Web.Enums;
using Newtonsoft.Json;

namespace FundingMock.Web.Models
{
    /// <summary>
    /// A calculation used to build up a funding line.
    /// </summary>
    public class Calculation
    {
        public Calculation()
        {
            AggregationType = AggregationType.Sum;
        }

        /// <summary>
        /// The name of the calculation. Used as a description within the model.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The template calculation id (i.e. a way to get to this property in the template).
        /// This value can be the same for multiple calculations within the hierarchy. 
        /// This indicates they will return the same value from the output.
        /// It allows input template to link calculations together, so a single calculation implemenation will be created instead of multiple depending on the hierarchy.
        /// 
        /// When templates are versioned, template IDs should be kept the same if they refer to the same thing, otherwise a new, unused ID should be used.
        /// </summary>
        public uint TemplateCalculationId { get; set; }

        /// <summary>
        /// The value the calculation is resulting in.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// The way the value should show.
        /// </summary>
        [EnumDataType(typeof(CalculationValueFormat))]
        public CalculationValueFormat ValueFormat { get; set; }

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
        /// How the calculation should aggregate.
        /// </summary>
        [EnumDataType(typeof(AggregationType))]
        public AggregationType AggregationType { get; set; }

        /// <summary>
        /// Sub level calculations.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<Calculation> Calculations { get; set; }

        /// <summary>
        /// Reference data this these calculations depend on.
        /// </summary>
        public IEnumerable<ReferenceData> ReferenceData { get; set; }
    }
}