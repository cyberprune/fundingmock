using System;
using System.Collections.Generic;
using FundingMock.Web.Enums;
using FundingMock.Web.Models;
using Swashbuckle.AspNetCore.Filters;

namespace FundingMock.Web.Examples
{
    public class FeedBaseModelExample : IExamplesProvider
    {
        public object GetExamples()
        {
            var ukOffset = new TimeSpan(0, 0, 0);
            var fundingVersion = "1.0";

            var period = new FundingPeriod
            {
                Period = "AY1920",
                Name = "Academic year 2019-20",
                Type = FundingPeriodType.AY,
                StartDate = new DateTimeOffset(2019, 9, 1, 0, 0, 0, ukOffset),
                EndDate = new DateTimeOffset(2020, 8, 31, 0, 0, 0, ukOffset)
            };

            var templateVersion = "1.0";

            var stream = new FundingStream
            {
                Code = "PSG",
                Name = "PE + Sport Premium",
            };

            var schemaVersion = "1.0";

            var groupingOrg = new OrganisationGroup()
            {
                GroupTypeIdentifier = OrganisationGroupTypeIdentifier.LACode,
                GroupTypeClassification = OrganisationGroupTypeClassification.LegalEntity,
                GroupTypeCode = OrganisationGroupTypeCode.LocalAuthority,
                Name = "Camden",
                SearchableName = "Camden",
                IdentifierValue = "202",
                Identifiers = new List<OrganisationIdentifier>
                {
                    new OrganisationIdentifier
                    {
                        Type = OrganisationGroupTypeIdentifier.LACode,
                        Value = "203"
                    },
                    new OrganisationIdentifier
                    {
                        Type = OrganisationGroupTypeIdentifier.UKPRN,
                        Value = "12345678"
                    }
                }
            };

            var financialYearPeriod1920 = new FundingPeriod
            {
                Period = "FY1920",
                Name = "Financial Year 2019-20",
                Type = FundingPeriodType.FY,
                StartDate = new DateTimeOffset(2019, 4, 1, 0, 0, 0, ukOffset),
                EndDate = new DateTimeOffset(2020, 3, 30, 0, 0, 0, ukOffset)
            };

            var financialYearPeriod2021 = new FundingPeriod
            {
                Period = "FY2021",
                Name = "Financial Year 2020-21",
                Type = FundingPeriodType.FY,
                StartDate = new DateTimeOffset(2020, 4, 1, 0, 0, 0, ukOffset),
                EndDate = new DateTimeOffset(2021, 3, 30, 0, 0, 0, ukOffset)
            };

            return new FeedBaseModel
            {
                SchemaUri = "http://example.org/#schema",
                SchemaVersion = schemaVersion,
                Funding = new FundingFeed
                {
                    FundingStream = stream,
                    FundingPeriod = period,
                    OrganisationGroup = groupingOrg,
                    FundingVersion = fundingVersion,
                    TemplateVersion = "0.9",
                    ExternalPublicationDate = new DateTimeOffset(2019, 9, 1, 0, 0, 0, new TimeSpan(1, 0, 0)),
                    EarliestPaymentAvailableDate = DateTimeOffset.Now,

                    Status = FundingStatus.Released,
                    StatusChangedDate = DateTimeOffset.Now,
                    GroupingReason = GroupingReason.Payment,
                    FundingValue = new FundingValue
                    {
                        FundingLines = new List<FundingLine>
                                {
                                    new FundingLine
                                    {
                                        Name = "Total funding line",
                                        FundingLineCode = "TotalFundingLine",
                                        TemplateLineId = 1,
                                        Type = FundingLineType.Payment,
                                        Value = 1400,
                                        DistributionPeriods = new List<DistributionPeriod>()
                                        {

                                        },
                                        Calculations = new List<Calculation>
                                        {
                                            new Calculation
                                            {
                                                Name = "Number of pupils",
                                                Type = CalculationType.PupilNumber,
                                                TemplateCalculationId = 1,
                                                Value = "456",
                                                ValueFormat = CalculationValueFormat.Number,
                                                FormulaText = "Something * something",
                                                ReferenceData = new List<ReferenceData>
                                                {
                                                    new ReferenceData
                                                    {
                                                        Name = "Academic year 2018 to 2019 pupil number on roll",
                                                        Value = "1",
                                                        Format = ReferenceDataValueFormat.Number
                                                    }
                                                }
                                            }
                                        }
                                    }
                                },
                        TotalValue = 2400
                    },
                    ProviderFundings = new List<string>
                    {
                            $"{stream.Code}_{period.Period}_12345678_{fundingVersion}",
                            $"{stream.Code}_{period.Period}_12345679_2.0",
                            $"{stream.Code}_{period.Period}_12345680_{fundingVersion}",
                    }
                }
            };
        }
    }
}