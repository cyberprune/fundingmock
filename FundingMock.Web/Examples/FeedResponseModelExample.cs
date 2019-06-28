using System;
using System.Collections.Generic;
using FundingMock.Web.Enums;
using FundingMock.Web.Models;
using Swashbuckle.AspNetCore.Filters;

namespace FundingMock.Web.Samples
{
    public class FeedResponseModelExample : IExamplesProvider
    {
        public object GetExamples()
        {
            var ukOffset = new TimeSpan(0, 0, 0);
            var fundingVersion = "1.0";

            var period = new FundingPeriod
            {
                Code = "AY1920",
                Name = "Academic year 2019-20",
                Type = PeriodType.AcademicYear,
                StartDate = new DateTimeOffset(2019, 9, 1, 0, 0, 0, ukOffset),
                EndDate = new DateTimeOffset(2020, 8, 31, 0, 0, 0, ukOffset)
            };

            var templateVersion = "1.0";

            var stream = new StreamWithTemplateVersion
            {
                Code = "PESports",
                Name = "PE + Sport Premium",
                TemplateVersion = templateVersion,
            };

            var groupingOrg = new OrganisationGroup()
            {
                Type = OrganisationGroupType.LocalAuthority,
                Name = "Camden",
                SearchableName = "Camden",
                Identifiers = new List<OrganisationIdentifier>
                {
                    new OrganisationIdentifier
                    {
                        Type = OrganisationIdentifierType.LACode,
                        Value = "203"
                    },
                    new OrganisationIdentifier
                    {
                        Type = OrganisationIdentifierType.UKPRN,
                        Value = "12345678"
                    }
                }
            };

            var id = $"{stream.Code}_{period.Code}_{groupingOrg.Type}_{groupingOrg.Name}_{fundingVersion}";

            return new FeedResponseModel
            {
                Id = Guid.NewGuid().ToString(),
                Title = "MYESF/CFS shared funding model mock API",
                Author = new FeedResponseAuthor
                {
                    Email = "calculate-funding@education.gov.uk",
                    Name = "Calculate Funding Service"
                },
                Updated = DateTime.Now,
                Rights = "Copyright (C) 2019 Department for Education",
                Link = new FeedLink[]
                {
                    new FeedLink
                    {
                        Href = "http://example.org/api/feed",
                        Rel = "self"
                    }
                },
                AtomEntry = new FeedResponseContentModel[]
                {
                    new FeedResponseContentModel
                    {
                        Id = id,
                        Author = new FeedResponseAuthor
                        {
                            Email = "calculate-funding@education.gov.uk",
                            Name = "Calculate Funding Service"
                        },
                        Title = $"Feed response for {stream.Code} - {period.Code} - version {fundingVersion}",
                        Link = new FeedLink[]
                        {
                            new FeedLink
                            {
                                Href = $"http://example.org/api/feed/byId/{id}",
                                Rel = "self"
                            }
                        },
                        Updated = DateTime.Now,
                        Content = (FeedBaseModel)new FeedBaseModelExample().GetExamples()
                    }
                }
            };
        }
    }
}