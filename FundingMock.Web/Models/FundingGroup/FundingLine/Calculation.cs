using FundingMock.Web.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FundingMock.Web.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class Calculation
    {
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int TemplateCalculationId { get; set; }

        /// <summary>
        /// QUESTION is this neeeded? Lines and periods don't have a sequence
        /// </summary>
        public int Sequence { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal Value { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [EnumDataType(typeof(ValueFormat))]
        public ValueFormat ValueFormat { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [EnumDataType(typeof(CalculationType))]
        public CalculationType Type { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string FormulaText { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<Calculation> Calculations { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<ReferenceData> ReferenceData { get; set; }
    }
}