using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FundingMock.Web.Models
{
    /// <summary>
    /// A funding line.
    /// </summary>
    public class FundingLine
    {
        /// <summary>
        /// The name of a funding line (e.g. "Total funding line").
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The funding value in pence.
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// A unique ID (in terms of template, not data) for this funding line (e.g. 345).
        /// </summary>
        public int TemplateLineId { get; set; }

        /// <summary>
        /// The type of the funding line (e.g. paid on this basis, or informational only).
        /// </summary>
        [EnumDataType(typeof(FundingLineType))]
        public FundingLineType Type { get; set; }

        /// <summary>
        /// The periods that this funding line where paid in / are due to be paid in.
        /// </summary>
        public IEnumerable<FundingLinePeriod> Periods { get; set; }

        /// <summary>
        /// Calculations that make up this funding line.
        /// </summary>
        public IEnumerable<Calculation> Calculations { get; set; }


        /// <summary>
        /// Sub funding lines that make up this funding line.
        /// </summary>
        public IEnumerable<FundingLine> FundingLines { get; set; }
    }
}