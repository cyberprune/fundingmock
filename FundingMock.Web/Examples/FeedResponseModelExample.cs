using System;
using System.Collections.Generic;
using FundingMock.Web.Enums;
using FundingMock.Web.Models;
using Swashbuckle.AspNetCore.Filters;

namespace FundingMock.Web.Examples
{
    public class FeedResponseModelExample : IExamplesProvider
    {
        public object GetExamples()
        {
            var ukOffset = new TimeSpan(0, 0, 0);
            var fundingVersion = "1.0";

            var period = new FundingPeriod
            {
                Period = "AY1920",
                Name = "Academic year 2019-20",
                Type = PeriodType.AY,
                StartDate = new DateTimeOffset(2019, 9, 1, 0, 0, 0, ukOffset),
                EndDate = new DateTimeOffset(2020, 8, 31, 0, 0, 0, ukOffset)
            };

            var templateVersion = "1.0";

            var stream = new FundingStream
            {
                Code = "PESports",
                Name = "PE + Sport Premium",
            };

            var groupingOrg = new OrganisationGroup()
            {
                GroupTypeIdentifier = OrganisationGroupTypeIdentifier.LocalAuthorityCode,
                GroupTypeClassification = OrganisationGroupTypeClassification.LegalEntity,
                GroupTypeCode = OrganisationGroupTypeCode.LocalAuthority,
                IdentifierValue = "202",
                Name = "Camden",
                SearchableName = "Camden",
                Identifiers = new List<OrganisationIdentifier>
                {
                    new OrganisationIdentifier
                    {
                        Type = OrganisationTypeIdentifier.LACode,
                        Value = "203"
                    },
                    new OrganisationIdentifier
                    {
                        Type = OrganisationTypeIdentifier.UKPRN,
                        Value = "12345678"
                    }
                }
            };

            var id = $"{stream.Code}_{period.Period}_{groupingOrg.GroupTypeCode}_{groupingOrg.Name}_{fundingVersion}";

            var host = "http://example.org";

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
                Link = new List<FeedLink>
                {
                    new FeedLink
                    {
                        Href = $"{host}/api/funding/feed/2",
                        Rel = "self"
                    },
                    new FeedLink
                    {
                        Href = $"{host}/api/funding/feed/5",
                        Rel = "first"
                    },
                    new FeedLink
                    {
                        Href = $"{host}/api/funding/feed/1",
                        Rel = "last"
                    },
                    new FeedLink
                    {
                        Href = $"{host}/api/funding/feed/1",
                        Rel = "previous"
                    },
                    new FeedLink
                    {
                        Href = $"{host}/api/funding/feed/3",
                        Rel = "next"
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
                        Title = $"Feed response for {stream.Code} - {period.Period} - version {fundingVersion}",
                        Link = new FeedLink[]
                        {
                            new FeedLink
                            {
                                Href = $"{host}/api/funding/feed/byId/{id}",
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