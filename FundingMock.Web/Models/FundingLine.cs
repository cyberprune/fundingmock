using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FundingMock.Web.Models
{
    public class FundingLine
    {
        public string FundingLineName { get; set; }

        public int FundingLineValue { get; set; }

        public int FundingLineIdentifier { get; set; }

        [EnumDataType(typeof(FundingLineType))]
        public FundingLineType FundingLineType { get; set; }

        public IEnumerable<FundingLinePeriod> FundingLinePeriods { get; set; }
    }
}