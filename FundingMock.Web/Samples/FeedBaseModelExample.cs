using System;
using System.Collections.Generic;
using FundingMock.Web.Models;
using Swashbuckle.AspNetCore.Filters;

namespace FundingMock.Web.Samples
{
    public class FeedBaseModelExample : IExamplesProvider
    {
        public object GetExamples()
        {
            var ukOffset = new TimeSpan(0, 0, 0);
            var fundingVersion = "1.0";

            var period = new FundingPeriod
            {
                Code = "AY1920",
                Name = "Academic year 2019-20",
                Type = Enums.PeriodType.AcademicYear,
                StartDate = new DateTimeOffset(2019, 9, 1, 0, 0, 0, ukOffset),
                EndDate = new DateTimeOffset(2020, 8, 31, 0, 0, 0, ukOffset)
            };

            var templateVersion = "2.1";

            var stream = new StreamWithTemplateVersion
            {
                Code = "PESports",
                Name = "PE + Sport Premium",
                TemplateVersion = templateVersion,
            };

            var schemaVersion = "1.0";

            var groupingOrg = new OrganisationGroup()
            {
                Type = Enums.OrganisationType.LocalAuthority,
                Name = "Camden",
                SearchableName = "Camden",
                Identifiers = new List<OrganisationIdentifier>
                {
                    new OrganisationIdentifier
                    {
                        Type = Enums.OrganisationIdentifierType.LACode,
                        Value = "203"
                    },
                    new OrganisationIdentifier
                    {
                        Type = Enums.OrganisationIdentifierType.UKPRN,
                        Value = "12345678"
                    }
                }
            };

            var id = $"{stream.Code}_{period.Code}_{groupingOrg.Type}_{groupingOrg.Name}_{fundingVersion}";

            var financialYearPeriod1920 = new FundingPeriod
            {
                Code = "FY1920",
                Name = "Financial Year 2019-20",
                Type = Enums.PeriodType.FinancialYear,
                StartDate = new DateTimeOffset(2019, 4, 1, 0, 0, 0, ukOffset),
                EndDate = new DateTimeOffset(2020, 3, 30, 0, 0, 0, ukOffset)
            };

            var financialYearPeriod2021 = new FundingPeriod
            {
                Code = "FY2021",
                Name = "Financial Year 2020-21",
                Type = Enums.PeriodType.FinancialYear,
                StartDate = new DateTimeOffset(2020, 4, 1, 0, 0, 0, ukOffset),
                EndDate = new DateTimeOffset(2021, 3, 30, 0, 0, 0, ukOffset)
            };

            return new List<FeedBaseModel>
            {
                new FeedBaseModel
                {
                    SchemaUri = "http://example.org/#schema",
                    SchemaVersion = schemaVersion,
                    Funding = new FundingFeed
                    {
                        FundingStream = stream,
                        FundingPeriod = period,
                        OrganisationGroup = groupingOrg,
                        Id = id,
                        FundingVersion = fundingVersion,

                        ExternalPublicationDate = new DateTimeOffset(2019, 9, 1, 0, 0, 0, new TimeSpan(1, 0, 0)),
                        PaymentDate = DateTimeOffset.Now,

                        Status = FundingStatus.Released,
                        StatusChangedDate = DateTimeOffset.Now,
                        GroupingReason = GroupingReason.Payment,
                        FundingValue = new FundingValue
                        {
                            FundingValueByDistributionPeriod = new List<FundingValueByDistributionPeriod>
                            {
                                new FundingValueByDistributionPeriod
                                {
                                    DistributionPeriodCode = financialYearPeriod1920.Code,
                                    Value = 1400,
                                    FundingLines = new List<FundingLine>
                                    {
                                        new FundingLine
                                        {
                                            Name = "Total funding line",
                                            FundingLineCode = "TotalFundingLine",
                                            TemplateLineId = 1,
                                            Type = FundingLineType.Payment,
                                            Value = 1400,
                                            Periods = new List<FundingLinePeriod>
                                            {
                                                new FundingLinePeriod
                                                {
                                                    Occurence = 1,
                                                    Year = 2019,
                                                    TypeValue = "October",
                                                    ProfiledValue = 1400,
                                                    Type = FundingLinePeriodType.CalendarMonth,
                                                    PeriodCode = financialYearPeriod1920.Code
                                                }
                                            },
                                            Calculations = new List<Calculation>
                                            {
                                                new Calculation
                                                {
                                                    Name = "Number of pupils",
                                                    Type = Enums.CalculationType.PupilNumber,
                                                    TemplateCalculationId = 1,
                                                    Value = "456",
                                                    ValueFormat = Enums.CalculationValueFormat.Number,
                                                    FormulaText = "Something * something",
                                                    ReferenceData = new List<ReferenceData>
                                                    {
                                                        new ReferenceData
                                                        {
                                                            Name = "Academic year 2018 to 2019 pupil number on roll",
                                                            Value = "1",
                                                            Format = Enums.ReferenceDataValueFormat.Number
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                },
                                new FundingValueByDistributionPeriod
                                {
                                    DistributionPeriodCode = financialYearPeriod2021.Code,
                                    Value = 1000,
                                    FundingLines = new List<FundingLine>
                                    {
                                        new FundingLine
                                        {
                                            Name = "Total funding line",
                                            FundingLineCode = "TotalFundingLine2",
                                            TemplateLineId = 1,
                                            Type = FundingLineType.Payment,
                                            Value = 1000,
                                            Periods = new List<FundingLinePeriod>
                                            {
                                                new FundingLinePeriod
                                                {
                                                    Occurence = 1,
                                                    Year = 2020,
                                                    TypeValue = "April",
                                                    ProfiledValue = 1000,
                                                    Type = FundingLinePeriodType.CalendarMonth,
                                                    PeriodCode = financialYearPeriod2021.Code
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
                              $"{stream.Code}_{period.Code}_12345678_{fundingVersion}",
                              $"{stream.Code}_{period.Code}_12345679_2.0",
                              $"{stream.Code}_{period.Code}_12345680_{fundingVersion}",
                        },
                    }
                }
            };
        }
    }
}