using System.ComponentModel.DataAnnotations;
using FundingMock.Web.Enums;

namespace FundingMock.Web.Models
{
    public class ParentGrouping
    {
        [EnumDataType(typeof(OrganisationType))]
        public OrganisationType Type { get; set; }

        public string Id { get; set; }
    }
}