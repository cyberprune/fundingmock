﻿using FundingMock.Web.Enums;
using FundingMock.Web.Models;
using Swashbuckle.AspNetCore.Filters;

namespace FundingMock.Web.Examples
{
    public class FeedRequestExample : IExamplesProvider
    {
        public object GetExamples()
        {
            return new FeedRequestObject
            {
                PageSize = 2,
                Statuses = new FundingStatus[] { FundingStatus.Released },
                Ukprns = null,
                GroupingReasons = new GroupingReason[] { GroupingReason.Information },
                MinStatusChangeDate = new System.DateTime(2000, 1, 1),
                VariationReasons = null,
                FundingLineTypes = new OrganisationGroupingReason[] { OrganisationGroupingReason.Information, OrganisationGroupingReason.Payment },
                FundingPeriodStartYear = 2018,
                FundingPeriodEndYear = 2020,
                FundingPeriodCodes = new string[] { "FY1920" },
                FundingStreamCodes = new string[] { "DSG" },
                TemplateLineIds = null,

                OrganisationIdentifiers = null,
                OrganisationGroupIdentifiers = null,
                OrganisationGroupTypes = new OrganisationGroupTypeIdentifier[] { OrganisationGroupTypeIdentifier.RSCRegionCode },
                OrganisationTypes = new OrganisationGroupTypeIdentifier[] { OrganisationGroupTypeIdentifier.LACode }
            };
        }
    }
}