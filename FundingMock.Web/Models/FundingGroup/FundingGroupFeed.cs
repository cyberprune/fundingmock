using System.Collections.Generic;

namespace FundingMock.Web.Models
{
    /// <summary>
    /// A funding group (a parent grouping organisation - such as an LA, MAT, Region etc...).
    /// </summary>
    public class FundingGroupFeed : FundingGroup
    {
        /// <summary>
        /// The fundings (child organisation level lines, e.g. providers under an LA) that are grouped into this funding group.
        /// </summary>
        public IEnumerable<string> Fundings { get; set; }
    }
}