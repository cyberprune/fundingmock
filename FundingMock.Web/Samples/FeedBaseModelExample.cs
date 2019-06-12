using FundingMock.Web.Models;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;

namespace FundingMock.Web.Samples
{
    public class FeedBaseModelExample : IExamplesProvider
    {
        public object GetExamples()
        {
            var ukOffset = new TimeSpan(0, 0, 0);
            var fundingVersion = 1;

            var period = new Period
            {
                Code = "AY1920",
                Name = "Academic year 2019-20",
                Type = Enums.PeriodType.AcademicYear,
                StartDate = new DateTimeOffset(2019, 9, 1, 0, 0, 0, ukOffset),
                EndDate = new DateTimeOffset(2020, 8, 31, 0, 0, 0, ukOffset)
            };

            var stream = new Stream
            {
                Code = "PESports",
                Name = "PE + Sport Premium"
            };

            var schemaVersion = 1;
            var templateVersion = 1;
            var groupingOrg = new GroupingOrganisation()
            {
                Type = Enums.OrganisationType.LocalAuthority,
                Name = "Camden",
                Identifiers = new List<OrganisationIdentifier>
                {
                    new OrganisationIdentifier
                    {
                        Type = Enums.OrganisationIdentifierType.LECode,
                        Value = "203"
                    },
                    new OrganisationIdentifier
                    {
                        Type = Enums.OrganisationIdentifierType.UKPRN,
                        Value = "12345678"
                    }
                }
            };

            var id = $"schema:v{schemaVersion}/{stream.Code}/template:v{templateVersion}/{groupingOrg.Name}/{period.Code}/funding:v{fundingVersion}";

            var financialYearPeriod1920 = new Period
            {
                Code = "FY1920",
                Name = "Financial Year 2019-20",
                Type = Enums.PeriodType.FinancialYear,
                StartDate = new DateTimeOffset(2019, 4, 1, 0, 0, 0, ukOffset),
                EndDate = new DateTimeOffset(2020, 3, 30, 0, 0, 0, ukOffset)
            };

            var financialYearPeriod2021 = new Period
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
                    SchemaVersion = schemaVersion,
                    FundingGroup = new FundingGroup
                    {
                        TemplateVersion = templateVersion,
                        Stream = stream,
                        Period = period,
                        GroupingOrganisation = groupingOrg,
                        Id = id,
                        FundingVersion = fundingVersion,

                        ExternalPublicationDate = new DateTimeOffset(2019, 9, 1, 0, 0, 0, new TimeSpan(1, 0, 0)),
                        PaymentDate = DateTimeOffset.Now,

                        Status = FundingStatus.Published,
                        StatusChangedDate = DateTimeOffset.Now,
                        GroupingReason = GroupingReason.Payment,
                        FundingValue = new FundingValue
                        {
                            DistributionPeriods = new List<FundingValueByDistributionPeriod>
                            {
                                new FundingValueByDistributionPeriod
                                {
                                    Code = financialYearPeriod1920.Code,
                                    Name = financialYearPeriod1920.Name,
                                    StartDate = financialYearPeriod1920.StartDate,
                                    EndDate = financialYearPeriod1920.EndDate,
                                    Value = 1400,
                                    FundingLines = new List<FundingLine>
                                    {
                                        new FundingLine
                                        {
                                            Name = "Total funding line",
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
                                                    ProfilingValue = 1400,
                                                    Type = FundingLinePeriodType.CalendarMonth,
                                                    Period = financialYearPeriod1920
                                                }
                                            },
                                            IsProfiled = false,
                                            Calculations = new List<Calculation>
                                            {
                                                new Calculation
                                                {
                                                    Name = "Number of pupils",
                                                    Type = Enums.CalculationType.PupilNumber,
                                                    Sequence = 1,
                                                    TemplateCalculationId = 1,
                                                    Value = 456,
                                                    ValueFormat = Enums.ValueFormat.Number,
                                                    FormulaText = "Something * something",
                                                    ReferenceData = new List<ReferenceData>
                                                    {
                                                        new ReferenceData
                                                        {
                                                            Name = "Academic year 2018 to 2019 pupil number on roll",
                                                            Value = 1,
                                                            Format = Enums.ValueFormat.Number,
                                                            Sequence = 1
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                },
                                new FundingValueByDistributionPeriod
                                {
                                    Code = financialYearPeriod2021.Code,
                                    Name = financialYearPeriod2021.Name,
                                    StartDate = financialYearPeriod2021.StartDate,
                                    EndDate = financialYearPeriod2021.EndDate,
                                    Value = 1000,
                                    FundingLines = new List<FundingLine>
                                    {
                                        new FundingLine
                                        {
                                            Name = "Total funding line",
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
                                                    ProfilingValue = 1000,
                                                    Type = FundingLinePeriodType.CalendarMonth,
                                                    Period = financialYearPeriod2021
                                                }
                                            }
                                        }
                                    }
                                }
                            },
                            TotalValue = 2400
                        },
                        Fundings = new List<Funding>
                        {
                            new Funding
                            {
                                Id = $"{id}/Example School 1",
                                FundingVersion = fundingVersion,

                                Period = period,
                                Stream = stream,
                                Organisation = new Organisation
                                {
                                    Name = "Example School 1",
                                    ProviderType = "School",
                                    Identifiers = new List<OrganisationIdentifier>
                                    {
                                        new OrganisationIdentifier
                                        {
                                            Type = Enums.OrganisationIdentifierType.URN,
                                            Value = "123453"
                                        },
                                        new OrganisationIdentifier
                                        {
                                            Type = Enums.OrganisationIdentifierType.UKPRN,
                                            Value = "87654321"
                                        }
                                    }
                                },
                                FundingValue = new FundingValue
                                {
                                    TotalValue = 1200,
                                    DistributionPeriods = new List<FundingValueByDistributionPeriod>
                                    {
                                        new FundingValueByDistributionPeriod
                                        {
                                            Code = financialYearPeriod1920.Code,
                                            Name = financialYearPeriod1920.Name,
                                            StartDate = financialYearPeriod1920.StartDate,
                                            EndDate = financialYearPeriod1920.EndDate,
                                            Value = 700,
                                            FundingLines = new List<FundingLine>
                                            {
                                                new FundingLine
                                                {
                                                    Name = "Total funding line",
                                                    TemplateLineId = 1,
                                                    Type = FundingLineType.Payment,
                                                    Value = 700,
                                                    Periods = new List<FundingLinePeriod>
                                                    {
                                                        new FundingLinePeriod
                                                        {
                                                            Occurence = 1,
                                                            Year = 2019,
                                                            TypeValue = "October",
                                                            ProfilingValue = 700,
                                                            Type = FundingLinePeriodType.CalendarMonth,
                                                            Period = financialYearPeriod1920
                                                        }
                                                    }
                                                }
                                            }
                                        },
                                        new FundingValueByDistributionPeriod
                                        {
                                            Code = financialYearPeriod2021.Code,
                                            Name = financialYearPeriod2021.Name,
                                            StartDate = financialYearPeriod2021.StartDate,
                                            EndDate = financialYearPeriod2021.EndDate,
                                            Value = 500,
                                            FundingLines = new List<FundingLine>
                                            {
                                                new FundingLine
                                                {
                                                    Name = "Total funding line",
                                                    TemplateLineId = 1,
                                                    Type = FundingLineType.Payment,
                                                    Value = 500,
                                                    Periods = new List<FundingLinePeriod>
                                                    {
                                                        new FundingLinePeriod
                                                        {
                                                            Occurence = 1,
                                                            Year = 2020,
                                                            TypeValue = "April",
                                                            ProfilingValue = 500,
                                                            Type = FundingLinePeriodType.CalendarMonth,
                                                            Period = financialYearPeriod2021
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            },
                            new Funding
                            {
                                Id = $"{id}/Example School 2",
                                FundingVersion = fundingVersion,

                                Period = period,
                                Stream = stream,
                                Organisation = new Organisation
                                {
                                    Name = "Example School 2",
                                    ProviderType = "School",
                                    Identifiers = new List<OrganisationIdentifier>
                                    {
                                        new OrganisationIdentifier
                                        {
                                            Type = Enums.OrganisationIdentifierType.URN,
                                            Value = "123453"
                                        },
                                        new OrganisationIdentifier
                                        {
                                            Type = Enums.OrganisationIdentifierType.UKPRN,
                                            Value = "87654321"
                                        }
                                    }
                                },
                                FundingValue = new FundingValue
                                {
                                    TotalValue = 1200,
                                    DistributionPeriods = new List<FundingValueByDistributionPeriod>
                                    {
                                        new FundingValueByDistributionPeriod
                                        {
                                            Code = financialYearPeriod1920.Code,
                                            Name = financialYearPeriod1920.Name,
                                            StartDate = financialYearPeriod1920.StartDate,
                                            EndDate = financialYearPeriod1920.EndDate,
                                            Value = 700,
                                            FundingLines = new List<FundingLine>
                                            {
                                                new FundingLine
                                                {
                                                    Name = "Total funding line",
                                                    TemplateLineId = 1,
                                                    Type = FundingLineType.Payment,
                                                    Value = 700,
                                                    Periods = new List<FundingLinePeriod>
                                                    {
                                                        new FundingLinePeriod
                                                        {
                                                            Occurence = 1,
                                                            Year = 2019,
                                                            TypeValue = "October",
                                                            ProfilingValue = 700,
                                                            Type = FundingLinePeriodType.CalendarMonth,
                                                            Period = financialYearPeriod1920
                                                        }
                                                    }
                                                }
                                            }
                                        },
                                        new FundingValueByDistributionPeriod
                                        {
                                            Code = financialYearPeriod2021.Code,
                                            Name = financialYearPeriod2021.Name,
                                            StartDate = financialYearPeriod2021.StartDate,
                                            EndDate = financialYearPeriod2021.EndDate,
                                            Value = 500,
                                            FundingLines = new List<FundingLine>
                                            {
                                                new FundingLine
                                                {
                                                    Name = "Total funding line",
                                                    TemplateLineId = 1,
                                                    Type = FundingLineType.Payment,
                                                    Value = 500,
                                                    Periods = new List<FundingLinePeriod>
                                                    {
                                                        new FundingLinePeriod
                                                        {
                                                            Occurence = 1,
                                                            Year = 2020,
                                                            TypeValue = "April",
                                                            ProfilingValue = 500,
                                                            Type = FundingLinePeriodType.CalendarMonth,
                                                            Period = financialYearPeriod2021
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}