using System;
using System.Collections.Generic;
using FundingMock.Web.Enums;
using FundingMock.Web.Models;
using Swashbuckle.AspNetCore.Filters;

namespace FundingMock.Web.Examples
{
    public class ProviderFundingModelExample : IExamplesProvider
    {
        public object GetExamples()
        {
            var ukOffset = new TimeSpan(0, 0, 0);
            var fundingVersion = "1-0";

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

            string providerId = "12345678";

            var groupingOrg = new OrganisationGroup()
            {
                GroupTypeIdentifier = OrganisationGroupTypeIdentifier.LACode,
                GroupTypeClassification = OrganisationGroupTypeClassification.LegalEntity,
                GroupTypeCode = OrganisationGroupTypeCode.LocalAuthority,
                IdentifierValue = "202",
                Name = "Camden",
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
                        Value = providerId
                    }
                }
            };


            var id = $"{stream.Code}_{period.Period}_{providerId}_{fundingVersion}";

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

            return new ProviderFunding
            {
                FundingVersion = fundingVersion,
                FundingPeriodId = period.Period,
                FundingStreamCode = stream.Code,
                Provider = new Provider
                {
                    Name = "Example School 1",
                    SearchableName = "ExampleSchool1",
                    ProviderDetails = new ProviderDetails()
                    {
                        DateClosed = null,
                        DateOpened = new DateTimeOffset(2012, 12, 2, 0, 0, 0, 0, TimeSpan.Zero),
                        PhaseOfEducation = "Secondary",
                        Status = "Open",
                        OpenReason = ProviderOpenReason.AcademyConverter,
                        CloseReason = null,
                        TrustName = "Trust Name",
                        TrustStatus = TrustStatus.SupportedByASingleAacademyTrust,
                        Postcode = "MOCK POSTCODE",
                        Town = "MOCK TOWN",
                    },
                    ProviderType = "Academies",
                    ProviderSubType = "Academy alternative provision converter",
                    ProviderVersionId = "3",
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
                            Value = providerId
                        }
                    },
                },
                VariationReasons = new List<VariationReason>()
                    {
                        VariationReason.NameFieldUpdated,
                        VariationReason.FundingUpdated,
                    },
                Successors = new List<ProviderInformationModel>()
                    {
                        new ProviderInformationModel()
                        {
                            ProviderVersionId = "pes2",
                            Ukprn = "2345333",
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
                                    FundingLineCode= "TotalFundingLine",
                                    TemplateLineId = 1,
                                    Type = OrganisationGroupingReason.Payment,
                                    Value = 700,
                                    DistributionPeriods = new List<DistributionPeriod>
                                    {
                                        new DistributionPeriod
                                        {
                                            DistributionPeriodId = financialYearPeriod1920.Period,
                                            Value = 200,
                                            ProfilePeriods = new List<ProfilePeriod>
                                            {
                                                new ProfilePeriod
                                                {
                                                    Occurrence = 1,
                                                    Year = 2019,
                                                    TypeValue = "October",
                                                    ProfiledValue = 200,
                                                    Type = ProfilePeriodType.CalendarMonth,
                                                    DistributionPeriodId = financialYearPeriod1920.Period
                                                }
                                            }
                                        },
                                        new DistributionPeriod
                                        {
                                            DistributionPeriodId = financialYearPeriod2021.Period,
                                            Value = 500,
                                            ProfilePeriods = new List<ProfilePeriod>
                                            {
                                                new ProfilePeriod
                                                {
                                                    Occurrence = 1,
                                                    Year = 2020,
                                                    TypeValue = "April",
                                                    ProfiledValue = 500,
                                                    Type = ProfilePeriodType.CalendarMonth,
                                                    DistributionPeriodId = financialYearPeriod2021.Period
                                                }
                                            },
                                        },
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
                                    }
                                }
                            },
                }
            };
        }
    }
}