using System;
using FundingMock.Web.Enums;
using Newtonsoft.Json;

namespace FundingMock.Web.Models
{
    /// <summary>
    /// Optional feed request parameters.
    /// </summary>
    public class FeedRequestObject
    {
        /// <summary>
        /// Optional - Page size to fetch.
        /// </summary>
        [JsonProperty("pageSize")]
        public int? PageSize { get; set; }

        /// <summary>
        /// Optional - The period start year.
        /// </summary>
        [JsonProperty("fundingPeriodStartYear")]
        public int? FundingPeriodStartYear { get; set; }

        /// <summary>
        /// Optional - The period end year.
        /// </summary>
        [JsonProperty("fundingPeriodEndYear")]
        public int? FundingPeriodEndYear { get; set; }

        /// <summary>
        /// Optional - Funding period codes to restrict to.
        /// </summary>
        [JsonProperty("fundingPeriodCodes")]
        public string[] FundingPeriodCodes { get; set; }

        /// <summary>
        /// Optional - Group identifiers to filter by.
        /// </summary>
        [JsonProperty("organisationGroupIdentifiers")]
        public ProviderIdentifier[] OrganisationGroupIdentifiers { get; set; }

        /// <summary>
        /// Optional - Group types to limit to.
        /// </summary>
        [JsonProperty("organisationGroupTypes")]
        public OrganisationGroupTypeIdentifier[] OrganisationGroupTypes { get; set; }

        /// <summary>
        /// Optional - Restrict returned identifiers by id.
        /// </summary>
        [JsonProperty("organisationIdentifiers")]
        public ProviderIdentifier[] OrganisationIdentifiers { get; set; }

        /// <summary>
        /// Optional - Organisation types to limit to.
        /// </summary>
        [JsonProperty("organisationTypes")]
        public OrganisationGroupTypeIdentifier[] OrganisationTypes { get; set; }

        /// <summary>
        /// Optional - Variation reasons to limit to.
        /// </summary>
        [JsonProperty("variationReasons")]
        public VariationReason[] VariationReasons { get; set; }

        /// <summary>
        /// Optional - UKPRNs to limit to.
        /// </summary>
        [JsonProperty("ukPrns")]
        public string[] Ukprns { get; set; }

        /// <summary>
        /// Optional - Grouping reasons to limit to (e.g. Information or Payment).
        /// </summary>
        [JsonProperty("groupingReasons")]
        public GroupingReason[] GroupingReasons { get; set; }

        /// <summary>
        /// Optional - Statuses to limit to (e.g. Released).
        /// </summary>
        [JsonProperty("statuses")]
        public FundingStatus[] Statuses { get; set; }

        /// <summary>
        /// Optional - Only get data that was changed after this date.
        /// </summary>
        [JsonProperty("minStatusChangedDate")]
        public DateTime? MinStatusChangeDate { get; set; }

        /// <summary>
        /// Optional - Stream codes to limit to.
        /// </summary>
        [JsonProperty("fundingStreamCodes")]
        public string[] FundingStreamCodes { get; set; }

        /// <summary>
        /// Optional - Only get funding lines with these types back.
        /// </summary>
        [JsonProperty("fundingLineTypes")]
        public FundingLineType[] FundingLineTypes { get; set; }

        /// <summary>
        /// Optional - Only get funding lines with these ids.
        /// </summary>
        [JsonProperty("templateLineIds")]
        public string[] TemplateLineIds { get; set; }
    }
}