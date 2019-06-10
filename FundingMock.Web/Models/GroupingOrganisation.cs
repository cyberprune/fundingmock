using System.Collections.Generic;

namespace FundingMock.Web.Models
{
    public class GroupingOrganisation
    {
        public string GroupingOrganisationType { get; set; }

        public string GroupingOrganisationName { get; set; }

        public IEnumerable<GroupedOrganisationIdentifier> GroupedOrganisationIdentifiers { get; set; }
    }
}