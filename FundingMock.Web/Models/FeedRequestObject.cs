using FundingMock.Web.Enums;
using System;

namespace FundingMock.Web.Models
{
    /// <summary>
    /// Optional feed request parameters.
    /// </summary>
    public class FeedRequestObject
    {
        // Feed level

        /// <summary>
        /// Page size to fetch.
        /// </summary>
        public int? PageSize { get; set; }

        // Funding period

        /// <summary>
        /// 
        /// </summary>
        public int? FundingPeriodStartYear { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int? FundingPeriodEndYear { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string[] FundingPeriodCodes { get; set; }

        // Organisation group level

        /// <summary>
        /// 
        /// </summary>
        public OrganisationIdentifier[] OrganisationGroupIdentifiers { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public OrganisationGroupType[] OrganisationGroupTypes { get; set; }

        // Organisation level

        /// <summary>
        /// 
        /// </summary>
        public OrganisationIdentifier[] OrganisationIdentifiers { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public OrganisationType[] OrganisationTypes { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public VariationReason[] VariationReasons { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string[] UkPrns { get; set; }

        // Funding level

        /// <summary>
        /// 
        /// </summary>
        public GroupingReason[] GroupingReasons { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public FundingStatus[] Statuses { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime? MinStatusChangeDate { get; set; }
        
        // Funding stream level 
        public string[] FundingStreamCodes { get; set; }

        // Line level            

        /// <summary>
        /// 
        /// </summary>
        public FundingLineType[] FundingLineTypes { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string[] TemplateLineIds { get; set; }
    }
}