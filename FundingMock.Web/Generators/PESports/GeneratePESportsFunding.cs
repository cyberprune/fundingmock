using System;
using System.Collections.Generic;
using System.Linq;
using FundingMock.Web.Controllers;
using FundingMock.Web.Enums;
using FundingMock.Web.Models;

namespace Sfa.Sfs.Mock.Generators
{
    /// <summary>
    /// Generate PE + Sports funding.
    /// </summary>
    public static class GeneratePESportsFunding
    {
        /// <summary>
        /// Lookup a feed entry by its id.
        /// </summary>
        /// <param name="id">The id to lookup from the feed.</param>
        /// <returns>A feed entry.</returns>
        public static FeedBaseModel GetFeedEntry(string id)
        {
            var data = GenerateFeed(null, null, null, null, null, null, null, null, null, null, null, null, null, null);

            foreach (var item in data)
            {
                if (item.Id == id)
                {
                    return item.Content;
                }
            }

            return null;
        }

        /// <summary>
        /// Generate a feed from CSV files (from a spreadsheet).
        /// </summary>
        /// <param name="fundingPeriodStartYear">Optional - </param>
        /// <param name="fundingPeriodEndYear">Optional - </param>
        /// <param name="fundingPeriodCodes">Optional - The period codes to limit to (e.g. AY1920).</param>
        /// <param name="organisationGroupIdentifiers">Optional - The group identifiers to limit by (e.g. UKPRN 12345678).</param>
        /// <param name="organisationGroupTypes">Optional - The group types to limit to (e.g. Region, LocalAuthority).</param>
        /// <param name="organisationIdentifiers">Optional - The organisation identifiers to limit to (e.g. UKPRN 12345678).</param>
        /// <param name="organisationTypes">Optional - The organisation types to return.</param>
        /// <param name="variationReasons">Optional - Filter to only organisations with these variation reasons types</param>
        /// <param name="ukprns">Optional - Only get these UKPRNs back.</param>
        /// <param name="groupingReasons">Optional - The grouping reasons we want to get back (e.g. Information and/or Payment).</param>
        /// <param name="statuses">Optional - The status of the funding (e.g. Released).</param>
        /// <param name="minStatusChangeDate">Optional - Only get records back that were changed after this date.</param>
        /// <param name="fundingLineTypes">Optional - limit the types of lines we want to get back (e.g. Information and/or Payment).</param>
        /// <param name="templateLineIds">Optional - Filter the lines to these ids only.</param>
        /// <returns>An array of FeedResponseContentModel objects./returns>
        public static FeedResponseContentModel[] GenerateFeed(int? fundingPeriodStartYear, int? fundingPeriodEndYear,
            string[] fundingPeriodCodes, ProviderIdentifier[] organisationGroupIdentifiers, OrganisationGroupTypeIdentifier[] organisationGroupTypes,
            ProviderIdentifier[] organisationIdentifiers, OrganisationGroupTypeIdentifier[] organisationTypes, VariationReason[] variationReasons,
            string[] ukprns, GroupingReason[] groupingReasons, FundingStatus[] statuses, DateTime? minStatusChangeDate,
            FundingLineType[] fundingLineTypes, string[] templateLineIds)
        {
            var totalList = new List<FeedResponseContentModel>();

            // Check period dates
            if ((fundingPeriodStartYear != null && fundingPeriodStartYear != 2019)
                || (fundingPeriodEndYear != null && fundingPeriodEndYear != 2020))
            {
                return totalList.ToArray();
            }

            // Check period codes
            if (fundingPeriodCodes?.Any() == true && fundingPeriodCodes?.Contains("AY1920") == false)
            {
                return totalList.ToArray();
            }

            // Check statuses
            if (statuses?.Any() == true && !statuses.Contains(FundingStatus.Released))
            {
                return totalList.ToArray();
            }

            // Check feed cut off date
            if (minStatusChangeDate != null && minStatusChangeDate.Value > new DateTime(2019, 3, 1))
            {
                return totalList.ToArray();
            }

            // If we only want information type, we are out of luck
            if (groupingReasons?.Any() == true && groupingReasons?.Contains(GroupingReason.Payment) == false)
            {
                return totalList.ToArray();
            }

            var fundingVersion = "1-0";
            var templateVersion = "1.0";
            var schemaVersion = "1.0";

            var ukOffset = new TimeSpan(0, 0, 0);

            var period = new FundingPeriod
            {
                Period = "AY1920",
                Name = "Academic year 2019-20",
                Type = PeriodType.AY,
                StartDate = new DateTimeOffset(2019, 9, 1, 0, 0, 0, ukOffset),
                EndDate = new DateTimeOffset(2020, 8, 31, 0, 0, 0, ukOffset)
            };

            var stream = new FundingStream
            {
                Code = "PESports",
                Name = "PE + Sport Premium",
            };

            var processFile = new ProcessPesportsCsv();

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

            var providerTypes = new List<string>
            {
                "MaintainedSchools",
                "Academies",
                "NonMaintainedSpecialSchools"
            };

            foreach (var providerType in providerTypes)
            {
                if ((providerType == "MaintainedSchools" || providerType == "NonMaintainedSpecialSchools")
                    && organisationGroupTypes?.Any() == true && organisationGroupTypes?.Contains(OrganisationGroupTypeIdentifier.LocalAuthorityCode) == false)
                {
                    continue;
                }
                else if (providerType == "Academies"
                    && organisationGroupTypes?.Any() == true && organisationGroupTypes?.Contains(OrganisationGroupTypeIdentifier.AcademyTrustCode) == false)
                {
                    continue;
                }

                var groupByLa = false;

                switch (providerType)
                {
                    case "MaintainedSchools":
                        groupByLa = true;

                        break;
                }

                var orgGroups = processFile.GetOrgsOrOrgGroups($"{providerType}.csv", groupByLa);

                totalList.AddRange(ProcessOrgGroups(orgGroups, providerType, financialYearPeriod1920, financialYearPeriod2021, period, stream, schemaVersion, fundingVersion,
                    organisationGroupIdentifiers, organisationIdentifiers, organisationTypes, variationReasons, ukprns, fundingLineTypes, templateLineIds)
                );
            }

            return totalList.ToArray();
        }

        /// <summary>
        /// Process org groups into feed models.
        /// </summary>
        /// <param name="orgGroups">List of org groups.</param>
        /// <param name="providerType">The provider types we are looking at.</param>
        /// <param name="financialYearPeriod1920">Data about the first financial period.</param>
        /// <param name="financialYearPeriod2021">Data about the second financial period.</param>
        /// <param name="period">Period to use.</param>
        /// <param name="stream">Stream to use.</param>
        /// <param name="schemaVersion">Schema version number.</param>
        /// <param name="fundingVersion">Funding version number.</param>
        /// <param name="organisationGroupIdentifiers">Optional - The group identifiers to limit by (e.g. UKPRN 12345678).</param>
        /// <param name="organisationIdentifiers">Optional - The organisation identifiers to limit to (e.g. UKPRN 12345678).</param>
        /// <param name="organisationTypes">Optional - The organisation types to return.</param>
        /// <param name="variationReasons">Optional - Filter to only organisations with these variation reasons types</param>
        /// <param name="ukprns">Optional - Only get these UKPRNs back.</param>
        /// <param name="fundingLineTypes">Optional - limit the types of lines we want to get back (e.g. Information and/or Payment).</param>
        /// <param name="templateLineIds">Optional - Filter the lines to these ids only.</param>
        /// <returns>A list of feed response models.</returns>
        private static List<FeedResponseContentModel> ProcessOrgGroups(List<OrgGroup> orgGroups, string providerType, FundingPeriod financialYearPeriod1920,
            FundingPeriod financialYearPeriod2021, FundingPeriod period, FundingStream stream, string schemaVersion, string fundingVersion,
            ProviderIdentifier[] organisationGroupIdentifiers, ProviderIdentifier[] organisationIdentifiers, OrganisationGroupTypeIdentifier[] organisationTypes,
            VariationReason[] variationReasons, string[] ukprns, FundingLineType[] fundingLineTypes, string[] templateLineIds)
        {
            var returnList = new List<FeedResponseContentModel>();

            // Limit by org group identifiers
            if (organisationGroupIdentifiers?.Any() == true)
            {
                foreach (var organisationGroupIdentifier in organisationGroupIdentifiers)
                {
                    orgGroups = orgGroups.Where(orgGroup => orgGroup.Type != organisationGroupIdentifier.Type.ToString()
                        || orgGroup.Code == organisationGroupIdentifier.Value).ToList();
                }
            }

            // Limit by org identifiers
            if (organisationIdentifiers?.Any() == true)
            {
                foreach (var orgGroup in orgGroups)
                {
                    orgGroup.Providers = orgGroup.Providers.Where(provider =>
                        organisationIdentifiers.Any(oi => oi.Type != ProviderTypeIdentifier.LACode || oi.Value == provider.LaEstablishmentNo)).ToList();
                }
            }

            foreach (var orgGroup in orgGroups)
            {
                OrganisationGroupTypeIdentifier orgTypeIdentifier = providerType == "NonMaintainedSpecialSchools" || providerType == "Academies" ?
                    (providerType == "NonMaintainedSpecialSchools" ? OrganisationGroupTypeIdentifier.LocalAuthorityCode : OrganisationGroupTypeIdentifier.AcademyTrustCode) : OrganisationGroupTypeIdentifier.LocalAuthorityCode;

                var ukprn = $"MOCKUKPRN{orgGroup.Code}";

                var groupingOrg = ConvertToOrganisationGroup(orgGroup, ukprn, orgTypeIdentifier, OrganisationGroupTypeCode.Provider, OrganisationGroupTypeClassification.LegalEntity);
                var id = $"{stream.Code}_{period.Period}_{groupingOrg.GroupTypeCode}_{ukprn}_{fundingVersion}";

                var data = new FeedBaseModel
                {
                    SchemaUri = "http://example.org/#schema",
                    SchemaVersion = schemaVersion,
                    Funding = new FundingFeed
                    {
                        FundingStream = stream,
                        FundingPeriod = period,
                        OrganisationGroup = groupingOrg,
                        FundingVersion = fundingVersion.Replace("-", "."),

                        ExternalPublicationDate = new DateTimeOffset(2019, 9, 1, 0, 0, 0, new TimeSpan(1, 0, 0)),
                        EarliestPaymentAvailableDate = DateTimeOffset.Now,
                        TemplateVersion = "1.0",
                        Status = FundingStatus.Released,
                        StatusChangedDate = DateTimeOffset.Now,
                        GroupingReason = GroupingReason.Payment,
                        ProviderFundings = GetProviderFundingIds(orgGroup, period, stream, fundingVersion, organisationTypes, variationReasons, ukprns),
                        FundingValue = new FundingValue
                        {
                            TotalValue = orgGroup.TotalAllocation,
                            FundingLines = new List<FundingLine>
                                    {
                                        new FundingLine
                                        {
                                            Name = "Total funding line",
                                            TemplateLineId = 1,
                                            Type = FundingLineType.Payment,
                                            Value = orgGroup.OctoberTotal,

                                            DistributionPeriods = new List<DistributionPeriod>
                                            {
                                                new DistributionPeriod
                                                {
                                                    DistributionPeriodId = financialYearPeriod1920.Period,
                                                    Value = orgGroup.OctoberTotal,
                                                    ProfilePeriods = new List<FundingLinePeriod>
                                                            {
                                                                new FundingLinePeriod
                                                                {
                                                                    Occurrence = 1,
                                                                    Year = 2019,
                                                                    TypeValue = "October",
                                                                    ProfiledValue = orgGroup.OctoberTotal,
                                                                    Type = FundingLinePeriodType.CalendarMonth,
                                                                    DistributionPeriodId = financialYearPeriod1920.Period
                                                                }
                                                            },
                                                },
                                                new DistributionPeriod
                                                {
                                                    DistributionPeriodId = financialYearPeriod2021.Period,
                                                    Value = orgGroup.AprilTotal,
                                                    ProfilePeriods = new List<FundingLinePeriod>
                                                    {
                                                        new FundingLinePeriod
                                                        {
                                                            Occurrence = 1,
                                                            Year = 2019,
                                                            TypeValue = "October",
                                                            ProfiledValue = orgGroup.OctoberTotal,
                                                            Type = FundingLinePeriodType.CalendarMonth,
                                                            DistributionPeriodId = financialYearPeriod1920.Period
                                                        }
                                                    },
                                                }
                                            }
                                        }
                                    },
                        }
                    },
                };

                if (fundingLineTypes?.Any() == true)
                {
                    //foreach (var dperiod in data.Funding.FundingValue.FundingValueByDistributionPeriod)
                    //{
                    //    dperiod.FundingLines = dperiod.FundingLines.Where(line => fundingLineTypes.Contains(line.Type)).ToList();

                    //    //TODO - filter at lower levels
                    //}
                }

                if (templateLineIds?.Any() == true)
                {
                    //foreach (var dperiod in data.Funding.FundingValue.FundingValueByDistributionPeriod)
                    //{
                    //    dperiod.FundingLines = dperiod.FundingLines.Where(line => templateLineIds.Contains(line.TemplateLineId.ToString())).ToList();

                    //    //TODO - filter at lower levels
                    //}
                }

                var host = "http://example.org";

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
                            Href = $"{host}/api/funding/feed/byId/{data.Funding.Id}",
                            Rel = "self"
                        }
                    }
                });
            };

            return returnList;
        }

        /// <summary>
        /// Convert an internal representation of an org into the response object.
        /// </summary>
        /// <param name="orgGroup"></param>
        /// <param name="ukprn"></param>
        /// <param name="organisationType"></param>
        /// <returns>An organsation group response objct.</returns>
        private static OrganisationGroup ConvertToOrganisationGroup(OrgGroup orgGroup, string ukprn, OrganisationGroupTypeIdentifier organisationType, OrganisationGroupTypeCode organisationGroupTypeCode, OrganisationGroupTypeClassification organisationGroupTypeCategory)
        {
            var identifiers = new List<OrganisationIdentifier>
            {
                new OrganisationIdentifier
                {
                    Type = OrganisationTypeIdentifier.UKPRN,
                    Value = ukprn
                }
            };

            if (organisationType == OrganisationGroupTypeIdentifier.LocalAuthorityCode)
            {
                identifiers.Add(new OrganisationIdentifier
                {
                    Type = OrganisationTypeIdentifier.LACode,
                    Value = orgGroup.Code
                });
            }

            return new OrganisationGroup()
            {
                GroupTypeIdentifier = organisationType,
                GroupTypeClassification = organisationGroupTypeCategory,
                GroupTypeCode = organisationGroupTypeCode,
                Name = orgGroup.Name,
                SearchableName = FundingController.SanitiseName(orgGroup.Name),
                Identifiers = identifiers
            };
        }

        /// <summary>
        /// Get the provider funding ids to return for a feed entry.
        /// </summary>
        /// <param name="orgGroup"></param>
        /// <param name="period"></param>
        /// <param name="stream"></param>
        /// <param name="fundingVersion"></param>
        /// <returns>A list of provider funding ids.</returns>
        private static List<string> GetProviderFundingIds(OrgGroup orgGroup, FundingPeriod period, FundingStream stream, string fundingVersion,
            OrganisationGroupTypeIdentifier[] organisationTypes, VariationReason[] variationReasons, string[] ukprns)
        {
            var returnList = new List<string>();

            // We don't have variation reasons yet
            if (variationReasons?.Any() == true)
            {
                return returnList;
            }

            // If we are asking for anything but local authorities, there won't be any results
            if (organisationTypes?.Any() == true && organisationTypes?.Contains(OrganisationGroupTypeIdentifier.LocalAuthorityCode) == false)
            {
                return returnList;
            }

            foreach (var provider in orgGroup.Providers)
            {
                var ukprn = $"MOCKUKPRN{provider.LaEstablishmentNo}";

                if (ukprns?.Any() == true && ukprns?.Contains(ukprn) == false)
                {
                    continue;
                }

                returnList.Add($"{stream.Code}_{period.Period}_{ukprn}_{fundingVersion}");
            }

            return returnList;
        }

        /// <summary>
        /// Get the provider funding from the spreadsheet
        /// </summary>
        /// <param name="id">The id to lookup.</param>
        /// <returns>A provider funding object.</returns>
        public static ProviderFunding GenerateProviderFunding(string id)
        {
            var idParts = id.Split('_');
            var code = idParts[2].Replace("MOCKUKPRN", string.Empty);

            var ukOffset = new TimeSpan(0, 0, 0);

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

            var processFile = new ProcessPesportsCsv();

            var orgGroups = new List<OrgGroup>();
            orgGroups.AddRange(processFile.GetOrgsOrOrgGroups("MaintainedSchools.csv", true));
            orgGroups.AddRange(processFile.GetOrgsOrOrgGroups("Academies.csv", false));
            orgGroups.AddRange(processFile.GetOrgsOrOrgGroups("MaintainedSchools.csv", false));

            foreach (var orgGroup in orgGroups)
            {
                foreach (var provider in orgGroup.Providers)
                {
                    if (provider.LaEstablishmentNo == code)
                    {
                        return GetProviderFunding(provider, financialYearPeriod1920, financialYearPeriod2021, period, stream, orgGroup.Type, orgGroup.Code);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Get provider funding from component parts.
        /// </summary>
        /// <param name="provider">A provider object.</param>
        /// <param name="financialYearPeriod1920">Period data for year1.</param>
        /// <param name="financialYearPeriod2021">Period data for year2.</param>
        /// <param name="period">Funding period.</param>
        /// <param name="stream">Data about a stream.</param>
        /// <param name="providerType">The type of provider (e.g. NonMaintainedSpecialSchools).</param>
        /// <param name="code">The code that identifies the provider (e.g. ukprn).</param>
        /// <returns>A provider funding object.</returns>
        private static ProviderFunding GetProviderFunding(Provider provider, FundingPeriod financialYearPeriod1920,
            FundingPeriod financialYearPeriod2021, FundingPeriod period, FundingStream stream, string providerType, string code)
        {
            var ukprn = $"MOCKUKPRN{provider.LaEstablishmentNo}";

            var identifiers = new List<ProviderIdentifier>
            {
                new ProviderIdentifier
                {
                    Type = ProviderTypeIdentifier.UKPRN,
                    Value = ukprn
                }
            };

            if (providerType != "NonMaintainedSpecialSchools" && providerType != "Academies")
            {
                identifiers.Add(new ProviderIdentifier
                {
                    Type = ProviderTypeIdentifier.LACode,
                    Value = provider.LaEstablishmentNo
                });
            }
            else
            {
                identifiers.Add(new ProviderIdentifier
                {
                    Type = ProviderTypeIdentifier.URN,
                    Value = code
                });

                identifiers.Add(new ProviderIdentifier
                {
                    Type = ProviderTypeIdentifier.DfeNumber,
                    Value = code
                });
            }

            var fundingVersion = "1-0";

            return new ProviderFunding
            {
                FundingVersion = fundingVersion.Replace("-", "."),
                FundingPeriodId = period.Period,
                FundingStreamCode = stream.Code,
                Provider = new FundingMock.Web.Models.Provider
                {
                    Name = provider.Name,
                    SearchableName = FundingController.SanitiseName(provider.Name),
                    ProviderDetails = new ProviderDetails()
                    {
                        DateClosed = null,
                        DateOpened = new DateTimeOffset(2012, 12, 2, 0, 0, 0, 0, TimeSpan.Zero),
                        PhaseOfEducation = "PhaseOfEducation",
                        Status = "Open",
                        OpenReason = ProviderOpenReason.NotRecorded,
                        CloseReason = null,
                        TrustName = null,
                        TrustStatus = TrustStatus.NotApplicable,
                        Postcode = "MOCK POSTCODE",
                        Town = "MOCK TOWN",
                    },
                    ProviderType = providerType,
                    ProviderSubType = "Provider SubType",
                    ProviderVersionId = "1.0",
                    OtherIdentifiers = identifiers,
                },
                FundingValue = new FundingValue
                {
                    TotalValue = provider.TotalAllocation, // 16200, // "Maintained Schools" -> F3
                    FundingLines = new List<FundingLine>
                            {
                                new FundingLine
                                {
                                    Name = "Total Allocation", // 
                                    TemplateLineId = 1,
                                    Type = FundingLineType.Payment,
                                    Value = provider.OctoberPayment, //9450, // "Maintained Schools"  -> G3
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
                                                    Value = provider.EligiblePupilsCount.ToString(), // "20",   //  "Maintained Schools"
                                                    Format = ReferenceDataValueFormat.Number
                                                },
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