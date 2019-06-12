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
        /// 
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
        /// QUESTION - This has the same types as GroupingReason. Are they the same thing? Is there a generic name for both we could use?
        /// </summary>
        [EnumDataType(typeof(FundingLineType))]
        public FundingLineType Type { get; set; }

        /// <summary>
        /// The periods that this funding line where paid in / are due to be paid in.
        /// </summary>
        public IEnumerable<FundingLinePeriod> Periods { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<Calculation> Calculations { get; set; }

        /// <summary>
        /// QUESTION what is this for?
        /// </summary>
        public bool IsProfiled { get; set; }
    }
}