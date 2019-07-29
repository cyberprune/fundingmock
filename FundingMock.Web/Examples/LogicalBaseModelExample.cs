using System;
using System.Collections.Generic;
using System.Linq;
using FundingMock.Web.Enums;
using FundingMock.Web.Models;
using Swashbuckle.AspNetCore.Filters;

namespace FundingMock.Web.Examples
{
    public class LogicalBaseModelExample : IExamplesProvider
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

            var templateVersion = "2.1";

            var stream = new FundingStream
            {
                Code = "PESports",
                Name = "PE + Sport Premium",
            };

            var schemaVersion = "1.0";

            var groupingOrg = new OrganisationGroup()
            {
                GroupTypeIdentifier = OrganisationGroupTypeIdentifier.LocalAuthorityCode,
                GroupTypeClassification = OrganisationGroupTypeClassification.LegalEntity,
                GroupTypeCode = OrganisationGroupTypeCode.LocalAuthority,
                Name = "Camden",
                SearchableName = "Camden",
                IdentifierValue = "203",
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

            var financialYearPeriod1920 = new FundingPeriod
            {
                Period = "FY1920",
                Name = "Financial Year 2019-20",
                Type = PeriodType.FY,
                StartDate = new DateTimeOffset(2019, 4, 1, 0, 0, 0, ukOffset),
                EndDate = new DateTimeOffset(2020, 3, 30, 0, 0, 0, ukOffset)
            };

            var financialYearPeriod2021 = new FundingPeriod
            {
                Period = "FY2021",
                Name = "Financial Year 2020-21",
                Type = PeriodType.FY,
                StartDate = new DateTimeOffset(2020, 4, 1, 0, 0, 0, ukOffset),
                EndDate = new DateTimeOffset(2021, 3, 30, 0, 0, 0, ukOffset)
            };

            LogicalBaseModel logicalBaseModel = new LogicalBaseModel
            {
                SchemaUri = "http://example.org/#schema",
                SchemaVersion = schemaVersion,
                Funding = new FundingProvider
                {
                    FundingStream = stream,
                    FundingPeriod = period,
                    OrganisationGroup = groupingOrg,
                    FundingVersion = fundingVersion,

                    ExternalPublicationDate = new DateTimeOffset(2019, 9, 1, 0, 0, 0, new TimeSpan(1, 0, 0)),
                    EarliestPaymentAvailableDate = DateTimeOffset.Now,

                    Status = FundingStatus.Released,
                    StatusChangedDate = DateTimeOffset.Now,
                    GroupingReason = GroupingReason.Payment,
                    TemplateVersion = "dsg1.0",
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
                                            DistributionPeriods = new List<DistributionPeriod>
                                            {
                                                new DistributionPeriod
                                                {
                                                    DistributionPeriodId = financialYearPeriod1920.Period,
                                                    Value = 2800,
                                                    ProfilePeriods = new List<FundingLinePeriod>
                                                    {
                                                        new FundingLinePeriod
                                                        {
                                                            Occurrence = 1,
                                                            Year = 2019,
                                                            TypeValue = "October",
                                                            ProfiledValue = 2800,
                                                            Type = FundingLinePeriodType.CalendarMonth,
                                                            DistributionPeriodId = financialYearPeriod1920.Period
                                                        }
                                                    },
                                                },
                                                new DistributionPeriod
                                                {
                                                    DistributionPeriodId = financialYearPeriod2021.Period,
                                                    Value = 2000,
                                                    ProfilePeriods = new List<FundingLinePeriod>
                                                    {
                                                        new FundingLinePeriod
                                                        {
                                                            Occurrence = 1,
                                                            Year = 2020,
                                                            TypeValue = "October",
                                                            ProfiledValue = 2000,
                                                            Type = FundingLinePeriodType.CalendarMonth,
                                                            DistributionPeriodId = financialYearPeriod2021.Period
                                                        }
                                                    },
                                                }
                                            },
                                        }
                                    },
                        TotalValue = 2400
                    },
                    ProviderFundings = new List<ProviderFunding>
                        {
                            new ProviderFunding
                            {
                                Id = $"{stream.Code}_{period.Period}_87654321_{fundingVersion}",
                                FundingVersion = fundingVersion,

                                FundingPeriodId = period.Period,
                                FundingStreamCode = stream.Code,
                                Provider = new Provider
                                {
                                    Name = "Example School 1",
                                    SearchableName = "ExampleSchool1",
                                    ProviderType = "School",
                                    Identifier = "87654321",
                                    ProviderDetails = new ProviderDetails()
                                    {

                                    },
                                    ProviderSubType = "School subtype 1",
                                    ProviderVersionId = "1819-1.0-pesports",
                                    OtherIdentifiers = new List<ProviderIdentifier>
                                    {
                                        new ProviderIdentifier
                                        {
                                            Type = ProviderTypeIdentifier.URN,
                                            Value = "123453"
                                        },
                                        new ProviderIdentifier
                                        {
                                            Type = ProviderTypeIdentifier.UKPRN,
                                            Value = "87654321"
                                        }
                                    }
                                },
                                FundingValue = new FundingValue
                                {
                                    TotalValue = 1200,
                                     FundingLines = new List<FundingLine>
                                            {
                                                new FundingLine
                                                {
                                                    Name = "Total funding line",
                                                    FundingLineCode = "TotalFundingLine",
                                                    TemplateLineId = 1,
                                                    Type = FundingLineType.Payment,
                                                    Value = 700,
                                                    DistributionPeriods = new List<DistributionPeriod>
                                                    {
                                                        new DistributionPeriod
                                                        {
                                                            DistributionPeriodId = financialYearPeriod1920.Period,
                                                            Value = 1400,
                                                            ProfilePeriods = new List<FundingLinePeriod>
                                                            {
                                                                new FundingLinePeriod
                                                                {
                                                                    Occurrence = 1,
                                                                    Year = 2019,
                                                                    TypeValue = "October",
                                                                    ProfiledValue = 1400,
                                                                    Type = FundingLinePeriodType.CalendarMonth,
                                                                    DistributionPeriodId = financialYearPeriod1920.Period
                                                                }
                                                            },
                                                        },
                                                        new DistributionPeriod
                                                        {
                                                            DistributionPeriodId = financialYearPeriod2021.Period,
                                                            Value = 1000,
                                                            ProfilePeriods = new List<FundingLinePeriod>
                                                            {
                                                                new FundingLinePeriod
                                                                {
                                                                    Occurrence = 1,
                                                                    Year = 2020,
                                                                    TypeValue = "October",
                                                                    ProfiledValue = 1000,
                                                                    Type = FundingLinePeriodType.CalendarMonth,
                                                                    DistributionPeriodId = financialYearPeriod2021.Period
                                                                }
                                                            },
                                                        }
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
                                                                    Format = ReferenceDataValueFormat.Number,
                                                                    TemplateReferenceId = 1,
                                                                }
                                                            }
                                                        },
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
                                                                    Format = ReferenceDataValueFormat.Number,
                                                                    TemplateReferenceId = 2,
                                                                }
                                                            }
                                                        }
                                                    },
                                                }
                                            },
                                },
                            },
                            new ProviderFunding
                            {
                                Id = $"{stream.Code}_{period.Period}_87654322_{fundingVersion}",
                                FundingVersion = fundingVersion,

                                FundingPeriodId = period.Period,
                                FundingStreamCode = stream.Code,
                                Provider = new Provider
                                {
                                    Name = "Example School 2",
                                    SearchableName = "ExampleSchool2",
                                    ProviderType = "School",
                                    ProviderSubType = "School subtype",
                                    OtherIdentifiers = new List<ProviderIdentifier>
                                    {
                                        new ProviderIdentifier
                                        {
                                            Type = ProviderTypeIdentifier.URN,
                                            Value = "123453"
                                        },
                                        new ProviderIdentifier
                                        {
                                            Type = ProviderTypeIdentifier.UKPRN,
                                            Value = "87654322"
                                        }
                                    },
                                    Identifier = "87654322",
                                    ProviderVersionId = "1.0",
                                    ProviderDetails = new ProviderDetails()
                                    {
                                        CloseReason = ProviderCloseReason.ChangeInStatus,
                                        DateClosed = DateTime.Now,
                                        DateOpened = DateTime.Now.AddYears(-10),
                                        OpenReason = ProviderOpenReason.FormerIndependent,
                                        PhaseOfEducation = "Primary",
                                        Postcode = "SSS222",
                                        Status = "Open",
                                        Town = "Town",
                                        TrustName = "Logical Trust",
                                        TrustStatus = TrustStatus.SupportedByAMultiAcademyTrust,
                                    },
                                },
                                FundingValue = new FundingValue
                                {
                                    TotalValue = 1200,
                                     FundingLines = new List<FundingLine>
                                            {
                                                new FundingLine
                                                {
                                                    Name = "Total funding line",
                                                    FundingLineCode = "TotalFundingLine",
                                                    TemplateLineId = 1,
                                                    Type = FundingLineType.Payment,
                                                    Value = 700,
                                                    DistributionPeriods = new List<DistributionPeriod>
                                                    {
                                                        new DistributionPeriod
                                                        {
                                                            DistributionPeriodId = financialYearPeriod1920.Period,
                                                            Value = 1400,
                                                            ProfilePeriods = new List<FundingLinePeriod>
                                                            {
                                                                new FundingLinePeriod
                                                                {
                                                                    Occurrence = 1,
                                                                    Year = 2019,
                                                                    TypeValue = "October",
                                                                    ProfiledValue = 1400,
                                                                    Type = FundingLinePeriodType.CalendarMonth,
                                                                    DistributionPeriodId = financialYearPeriod1920.Period
                                                                }
                                                            },
                                                        },
                                                        new DistributionPeriod
                                                        {
                                                            DistributionPeriodId = financialYearPeriod2021.Period,
                                                            Value = 1000,
                                                            ProfilePeriods = new List<FundingLinePeriod>
                                                            {
                                                                new FundingLinePeriod
                                                                {
                                                                    Occurrence = 1,
                                                                    Year = 2020,
                                                                    TypeValue = "October",
                                                                    ProfiledValue = 1000,
                                                                    Type = FundingLinePeriodType.CalendarMonth,
                                                                    DistributionPeriodId = financialYearPeriod2021.Period
                                                                }
                                                            },
                                                        }
                                                    },
                                                }
                                            },
                                }
                            }
                        }
                }
            };

            // Set all variation reasons for verification
            var allVariationReasons = Enum.GetValues(typeof(VariationReason));
            List<VariationReason> variationReasons = new List<VariationReason>();
            foreach (var variationReason in allVariationReasons)
            {
                variationReasons.Add((VariationReason)variationReason);
            }

            logicalBaseModel.Funding.ProviderFundings.First().VariationReasons = variationReasons;


            return logicalBaseModel;
        }
    }
}