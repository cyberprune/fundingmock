using System;
using System.Collections.Generic;
using FundingMock.Web.Enums;
using FundingMock.Web.Models;
using System.Linq;

namespace FundingMock.Web.Samples
{
    public static class GeneratePESportsFunding
    {
        public static FeedBaseModel GetFeedEntry(string id)
        {
            var data = GenerateFeed(int.MaxValue, null);

            foreach (var item in data)
            {
                if (item.Id == id)
                {
                    return item.Content;
                }
            }

            return null;
        }

        public static FeedResponseContentModel[] GenerateFeed(int pageSize, int? pageRef)
        {
            var fundingVersion = "1.0";
            var templateVersion = "1.0";
            var schemaVersion = "1.0";

            var ukOffset = new TimeSpan(0, 0, 0);

            var period = new FundingPeriod
            {
                Code = "AY1920",
                Name = "Academic year 2019-20",
                Type = PeriodType.AcademicYear, 
                StartDate = new DateTimeOffset(2019, 9, 1, 0, 0, 0, ukOffset),
                EndDate = new DateTimeOffset(2020, 8, 31, 0, 0, 0, ukOffset)
            };

            var stream = new StreamWithTemplateVersion
            {
                Code = "PESports",
                Name = "PE + Sport Premium",
                TemplateVersion = templateVersion,
            };

            var processFile = new ProcessFile();

            var financialYearPeriod1920 = new FundingPeriod
            {
                Code = "FY1920",
                Name = "Financial Year 2019-20",
                Type = PeriodType.FinancialYear,
                StartDate = new DateTimeOffset(2019, 4, 1, 0, 0, 0, ukOffset),
                EndDate = new DateTimeOffset(2020, 3, 30, 0, 0, 0, ukOffset)
            };

            var financialYearPeriod2021 = new FundingPeriod
            {
                Code = "FY2021",
                Name = "Financial Year 2020-21",
                Type = PeriodType.FinancialYear,
                StartDate = new DateTimeOffset(2020, 4, 1, 0, 0, 0, ukOffset),
                EndDate = new DateTimeOffset(2021, 3, 30, 0, 0, 0, ukOffset)
            };

            var providerTypes = new List<string>
            {
                "MaintainedSchools",
                "Academies",
                "MaintainedSchools"
            };

            var baseModels = new List<FeedResponseContentModel>();

            foreach (var providerType in providerTypes)
            {
                var groupByLa = false;

                switch (providerType)
                {
                    case "MaintainedSchools":
                        groupByLa = true;

                        break;
                }

                var orgGroups = processFile.GetOrgGroups($"{providerType}.csv", groupByLa);
                baseModels.AddRange(ProcessOrgGroups(orgGroups, providerType, financialYearPeriod1920, financialYearPeriod2021, period, stream, schemaVersion, fundingVersion));
            }

            return baseModels.Skip((pageRef ?? 0 * pageSize)).Take(pageSize).ToArray();
        }

        private static List<FeedResponseContentModel> ProcessOrgGroups(List<OrgGroup> orgGroups, string providerType, FundingPeriod financialYearPeriod1920, 
            FundingPeriod financialYearPeriod2021, FundingPeriod period, StreamWithTemplateVersion stream, string schemaVersion, string fundingVersion)
        {
            var returnList = new List<FeedResponseContentModel>();

            foreach (var orgGroup in orgGroups)
            {
                var orgType = providerType == "NonMaintainedSpecialSchools" || providerType == "Academies" ? OrganisationType.Provider : OrganisationType.LocalAuthority;

                var groupingOrg = ConvertToOrganisationGroup(orgGroup, orgGroup.Code, orgType);
                var id = $"{stream.Code}_{period.Code}_{groupingOrg.Type}_{groupingOrg.Name.Replace(" ", string.Empty)}_{fundingVersion}";

                var data = new FeedBaseModel
                {
                    SchemaUri = "http://example.org/#schema",
                    SchemaVersion = schemaVersion,
                    Funding = new FundingFeed
                    {
                        Id = id,

                        FundingStream = stream,
                        FundingPeriod = period,
                        OrganisationGroup = groupingOrg,
                        FundingVersion = fundingVersion,

                        ExternalPublicationDate = new DateTimeOffset(2019, 9, 1, 0, 0, 0, new TimeSpan(1, 0, 0)),
                        PaymentDate = DateTimeOffset.Now,

                        Status = FundingStatus.Released,
                        StatusChangedDate = DateTimeOffset.Now,
                        GroupingReason = GroupingReason.Payment,
                        ProviderFundings = GetProviderFundingIds(orgGroup, period, stream, fundingVersion),
                        FundingValue = new FundingValue
                        {
                            TotalValue = orgGroup.TotalAllocation,

                            FundingValueByDistributionPeriod = new List<FundingValueByDistributionPeriod>
                            {
                                new FundingValueByDistributionPeriod
                                {
                                    DistributionPeriodCode = financialYearPeriod1920.Code,
                                    Value = orgGroup.OctoberTotal,
                                    FundingLines = new List<FundingLine>
                                    {
                                        new FundingLine
                                        {
                                            Name = "Total funding line",
                                            TemplateLineId = 1,
                                            Type = FundingLineType.Payment,
                                            Value = orgGroup.OctoberTotal,
                                            ProfilePeriods = new List<FundingLinePeriod>
                                            {
                                                new FundingLinePeriod
                                                {
                                                    Occurence = 1,
                                                    Year = 2019,
                                                    TypeValue = "October",
                                                    ProfiledValue = orgGroup.OctoberTotal,
                                                    Type = FundingLinePeriodType.CalendarMonth,
                                                    PeriodCode = financialYearPeriod1920.Code
                                                }
                                            },
                                        }
                                    }
                                },
                                new FundingValueByDistributionPeriod
                                {
                                    DistributionPeriodCode = financialYearPeriod2021.Code,
                                    Value = orgGroup.AprilTotal,
                                    FundingLines = new List<FundingLine>
                                    {
                                        new FundingLine
                                        {
                                            Name = "Total funding line",
                                            TemplateLineId = 2,
                                            Type = FundingLineType.Payment,
                                            Value = orgGroup.AprilTotal,
                                            ProfilePeriods = new List<FundingLinePeriod>
                                            {
                                                new FundingLinePeriod
                                                {
                                                    Occurence = 1,
                                                    Year = 2020,
                                                    TypeValue = "April",
                                                    ProfiledValue = orgGroup.AprilTotal,
                                                    Type = FundingLinePeriodType.CalendarMonth,
                                                    PeriodCode = financialYearPeriod2021.Code
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                };

                returnList.Add(new FeedResponseContentModel
                {
                    Content = data,
                    Id = data.Funding.Id,
                    Author = new FeedResponseAuthor
                    {
                        Email = "calculate-funding@education.gov.uk",
                        Name = "Calculate Funding Service"
                    },
                    Title = data.Funding.Id,
                    Updated = DateTime.Now,
                    Link = new FeedLink[]
                    {
                        new FeedLink
                        {
                            Href = $"#/{data.Funding.Id}",
                            Rel = "self"
                        }
                    }
                });
            };

            return returnList;
        }

        private static OrganisationGroup ConvertToOrganisationGroup(OrgGroup orgGroup, string ukprn, OrganisationType organisationType)
        {
            var identifiers = new List<OrganisationIdentifier>
            {
                new OrganisationIdentifier
                {
                    Type = OrganisationIdentifierType.UKPRN,
                    Value = ukprn
                }
            };

            if (organisationType == OrganisationType.LocalAuthority)
            {
                identifiers.Add(new OrganisationIdentifier
                {
                    Type = OrganisationIdentifierType.LACode,
                    Value = orgGroup.Code
                });
            }

            return new OrganisationGroup()
            {
                Type = organisationType,
                Name = orgGroup.Name,
                SearchableName = orgGroup.Name.Replace(" ", string.Empty),
                Identifiers = identifiers
            };
        }

        private static List<string> GetProviderFundingIds(OrgGroup orgGroup, FundingPeriod period, Stream stream, string fundingVersion)
        {
            var returnList = new List<string>();

            foreach (var provider in orgGroup.Providers)
            {
                returnList.Add($"{stream.Code}_{period.Code}_{provider.LaEstablishmentNo}_{fundingVersion}");
            }

            return returnList;
        }

        public static ProviderFunding GenerateProviderFunding(string id)
        {
            var idParts = id.Split('_');
            var code = idParts[2].Replace("MOCKUKPRN", string.Empty);

            var ukOffset = new TimeSpan(0, 0, 0);

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

            var financialYearPeriod1920 = new FundingPeriod
            {
                Code = "FY1920",
                Name = "Financial Year 2019-20",
                Type = PeriodType.FinancialYear,
                StartDate = new DateTimeOffset(2019, 4, 1, 0, 0, 0, ukOffset),
                EndDate = new DateTimeOffset(2020, 3, 30, 0, 0, 0, ukOffset)
            };

            var financialYearPeriod2021 = new FundingPeriod
            {
                Code = "FY2021",
                Name = "Financial Year 2020-21",
                Type = PeriodType.FinancialYear,
                StartDate = new DateTimeOffset(2020, 4, 1, 0, 0, 0, ukOffset),
                EndDate = new DateTimeOffset(2021, 3, 30, 0, 0, 0, ukOffset)
            };

            var processFile = new ProcessFile();

            var orgGroups = new List<OrgGroup>();
            orgGroups.AddRange(processFile.GetOrgGroups("MaintainedSchools.csv", true));
            orgGroups.AddRange(processFile.GetOrgGroups("Academies.csv", false));
            orgGroups.AddRange(processFile.GetOrgGroups("MaintainedSchools.csv", false));

            foreach (var orgGroup in orgGroups)
            {
                foreach (var provider in orgGroup.Providers)
                {
                    if (provider.LaEstablishmentNo == code)
                    {
                        return GetProvider(provider, financialYearPeriod1920, financialYearPeriod2021, period, stream, orgGroup.Type, orgGroup.Code);
                    }
                }
            }

            return null;
        }

        private static ProviderFunding GetProvider(Provider provider, FundingPeriod financialYearPeriod1920,
            FundingPeriod financialYearPeriod2021, FundingPeriod period, Stream stream, string providerType, string code)
        {
            var identifiers = new List<OrganisationIdentifier>
            {
                new OrganisationIdentifier
                {
                    Type = OrganisationIdentifierType.UKPRN,
                    Value = code
                }
            };

            if (providerType != "NonMaintainedSpecialSchools" && providerType != "Academies")
            {
                identifiers.Add(new OrganisationIdentifier
                {
                    Type = OrganisationIdentifierType.LACode,
                    Value = provider.LaEstablishmentNo
                });
            }
            else
            {
                identifiers.Add(new OrganisationIdentifier
                {
                    Type = OrganisationIdentifierType.URN,
                    Value = code
                });

                identifiers.Add(new OrganisationIdentifier
                {
                    Type = OrganisationIdentifierType.DfeNumber,
                    Value = code
                });
            }

            var fundingVersion = "1.0";

            return new ProviderFunding
            {
                Id = $"{stream.Code}_{period.Code}_{provider.LaEstablishmentNo}_{fundingVersion}",
                FundingPeriodCode = period.Code,
                FundingStreamCode = stream.Code,
                Organisation = new Organisation
                {
                    Name = provider.SchoolName,
                    SearchableName = provider.SchoolName.Replace(" ", string.Empty),

                    OrganisationDetails = new OrganisationDetails()
                    {
                        DateClosed = null,
                        DateOpened = new DateTimeOffset(2012, 12, 2, 0, 0, 0, 0, TimeSpan.Zero),
                        PhaseOfEducation = "PhaseOfEducation",
                        Status = "Open",
                        OpenReason = ProviderOpenReason.NotRecorded,
                        CloseReason = null,
                        TrustName = null,
                        TrustStatus = TrustStatus.NotApplicable
                    },
                    ProviderType = providerType,
                    ProviderSubType = "Provider SubType",
                    ProviderVersionId = "1.0",
                    Identifiers = identifiers
                },
                FundingValue = new FundingValue
                {
                    TotalValue = provider.TotalAllocation, // 16200, // "Maintained Schools" -> F3
                    FundingValueByDistributionPeriod = new List<FundingValueByDistributionPeriod>
                    {
                        new FundingValueByDistributionPeriod
                        {
                            DistributionPeriodCode = financialYearPeriod1920.Code,
                            Value = provider.OctoberPayment, //9450,  // "Maintained Schools" -> G3
                            FundingLines = new List<FundingLine>
                            {
                                new FundingLine
                                {
                                    Name = "Total Allocation", // 
                                    TemplateLineId = 1,
                                    Type = FundingLineType.Payment,
                                    Value = provider.OctoberPayment, //9450, // "Maintained Schools"  -> G3
                                    ProfilePeriods = new List<FundingLinePeriod>
                                    {
                                        new FundingLinePeriod // ProfiorPeriods
                                        {
                                            Occurence = 1,
                                            Year = 2019,
                                            TypeValue = "October",
                                            ProfiledValue = provider.OctoberPayment, //9450, // "Maintained Schools"  -> G3
                                            Type = FundingLinePeriodType.CalendarMonth,
                                            PeriodCode = financialYearPeriod1920.Code
                                        }
                                    },
                                    Calculations = new List<Calculation>
                                    {
                                        new Calculation
                                        {
                                            Name = "Total Allocation",
                                            Type = CalculationType.Cash,
                                            TemplateCalculationId = 1,
                                            Value = provider.TotalAllocation, //"16200",  //  "Maintained Schools" 
                                            ValueFormat = CalculationValueFormat.Currency,
                                            FormulaText = "School with pupils with less than 17 eligible pupils (X) = 1000 * X School with pupils with more than 16 eligible pupils (X) =((1 * 16000) + (10 * X))",
                                            ReferenceData = new List<ReferenceData>
                                            {
                                                new ReferenceData
                                                {
                                                    Name = "Eligible pupils",
                                                    Value = provider.EligiblePupils.ToString(), // "20",   //  "Maintained Schools"
                                                    Format = ReferenceDataValueFormat.Number
                                                },
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        new FundingValueByDistributionPeriod
                        {
                            DistributionPeriodCode = financialYearPeriod2021.Code,
                            Value = 6750, // "Maintained Schools" -> H3
                            FundingLines = new List<FundingLine>
                            {
                                new FundingLine
                                {
                                    Name = "April payment",// "Maintained Schools"  
                                    TemplateLineId = 2,
                                    Type = FundingLineType.Payment,
                                    Value = provider.AprilPayment, //6750,  // "Maintained Schools" -> H3
                                    ProfilePeriods = new List<FundingLinePeriod>
                                    {
                                        new FundingLinePeriod
                                        {
                                            Occurence = 1,
                                            Year = 2020,
                                            TypeValue = "April",
                                            ProfiledValue = provider.AprilPayment,  //6750, // "Maintained Schools" -> H3
                                            Type = FundingLinePeriodType.CalendarMonth,
                                            PeriodCode = financialYearPeriod2021.Code
                                        }
                                    },
                                    Calculations = new List<Calculation>
                                    {
                                        new Calculation
                                        {
                                            Name = "Total Allocation",
                                            Type = Enums.CalculationType.Cash,
                                            TemplateCalculationId = 1,
                                            Value = provider.TotalAllocation, //"16200",  //  "Maintained Schools" 
                                            ValueFormat = CalculationValueFormat.Currency,
                                            FormulaText = "School with pupils with less than 17 eligible pupils (X) = 1000 * X School with pupils with more than 16 eligible pupils (X) =((1 * 16000) + (10 * X))",
                                            ReferenceData = new List<ReferenceData>
                                            {
                                                new ReferenceData
                                                {
                                                    Name = "Eligible pupils",
                                                    Value = provider.EligiblePupils.ToString(), // "20",   //  "Maintained Schools"
                                                    Format = ReferenceDataValueFormat.Number
                                                },
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