using System.Runtime.Serialization;

namespace FundingMock.Web.Models
{
    public class BaseModel
    {
        [DataMember(Order = 1)]
        public FundingGroup FundingGroup { get; set; }

        /// <summary>
        /// TODO : Verify this
        /// </summary>
        [DataMember(Order = 0)]
        public int SchemaVersion { get; set; }
    }
}
