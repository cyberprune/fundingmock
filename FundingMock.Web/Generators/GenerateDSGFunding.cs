using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using ExcelDataReader;
using FundingMock.Web.Controllers;
using FundingMock.Web.Enums;
using FundingMock.Web.Models;

namespace Sfa.Sfs.Mock.Generators
{
    /// <summary>
    /// Generate DSG funding.
    /// </summary>
    public static class GenerateDSGFunding
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
        /// Generate a feed from a spreadsheet.
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
        /// <returns>An array of FeedResponseContentModel objects.</returns>
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
            if (fundingPeriodCodes?.Any() == true && fundingPeriodCodes?.Contains("FY1920") == false)
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

            // Check feed cut off date
            if (minStatusChangeDate != null && minStatusChangeDate.Value > new DateTime(2019, 3, 7))
            {
                return totalList.ToArray();
            }

            var spreadsheet = GetSpreadsheet();

            var allLas = GetLAs(spreadsheet);
            var las = organisationGroupTypes?.Any() != true || organisationGroupTypes?.Contains(OrganisationGroupTypeIdentifier.LocalAuthorityCode) != false ?
                allLas.Where(item =>
                    ukprns == null
                    || ukprns.Length == 0
                    || item.Code == ukprns[0]
                    || item.Name.Equals(ukprns[0], StringComparison.InvariantCultureIgnoreCase)).ToList() : new List<Org>();

            if (organisationGroupIdentifiers?.Any() == true && organisationGroupIdentifiers?.Any(ogi => ogi.Type == ProviderTypeIdentifier.LACode) == true)
            {
                foreach (var la in las)
                {
                    // TODO
                }
            }

            var allRegions = GetRegions(spreadsheet);
            var regions = organisationGroupTypes?.Any() != true || organisationGroupTypes?.Contains(OrganisationGroupTypeIdentifier.RegionCode) != false ?
                allRegions.Where(item =>
                    ukprns == null
                    || ukprns.Length == 0
                    || item.Name.Equals(ukprns[0], StringComparison.InvariantCultureIgnoreCase)).ToList() : new List<Org>();

            var organisations = las.ToList(); // Shallow copy
            organisations.AddRange(regions);

            if (organisationGroupTypes?.Any() != true || organisationGroupTypes?.Contains(OrganisationGroupTypeIdentifier.LocalGovernmentGroupCode) != false)
            {
                if (groupingReasons?.Any() != true || groupingReasons?.Contains(GroupingReason.Information) == true)
                {
                    organisations.Add(new Org { Name = "METROPOLITAN AUTHORITIES", SpreadsheetRowNumber = 156, Type = Type.LocalGovernmentGroup });
                    organisations.Add(new Org { Name = "UNITARY AUTHORITIES", SpreadsheetRowNumber = 157, Type = Type.LocalGovernmentGroup });
                    organisations.Add(new Org { Name = "UPPER TIER AUTHORITIES", SpreadsheetRowNumber = 158, Type = Type.LocalGovernmentGroup });
                }
            }

            var ukOffset = new TimeSpan(0, 0, 0);

            var period = new FundingPeriod
            {
                Period = "FY1920",
                Name = "Financial year 2019-20",
                Type = PeriodType.FY,
                StartDate = new DateTimeOffset(2019, 4, 1, 0, 0, 0, ukOffset),
                EndDate = new DateTimeOffset(2020, 3, 31, 0, 0, 0, ukOffset)
            };

            var templateVersion = "1.0";

            var stream = new FundingStream
            {
                Code = "DSG",
                Name = "Dedicated Schools Grant",
            };

            var fundingVersion = "1-0";
            var schemaUri = "http://example.org/#schema";
            var schemaVersion = "1.0";

            foreach (var organisation in organisations)
            {
                var identifiers = new List<OrganisationIdentifier>();

                if (organisation.Type == Type.LA)
                {
                    identifiers.Add(
                        new OrganisationIdentifier
                        {
                            Type = OrganisationTypeIdentifier.LACode,
                            Value = organisation.Code
                        }
                    );

                    identifiers.Add(
                        new OrganisationIdentifier
                        {
                            Type = OrganisationTypeIdentifier.UKPRN,
                            Value = organisation.UKPRN
                        }
                    );
                }

                var groupingOrg = new OrganisationGroup
                {
                    Name = organisation.Name,
                    GroupTypeIdentifier = organisation.Type == Type.LA ? OrganisationGroupTypeIdentifier.LocalAuthorityCode
                        : (organisation.Type == Type.LocalGovernmentGroup ? OrganisationGroupTypeIdentifier.LocalGovernmentGroupCode : OrganisationGroupTypeIdentifier.RegionCode),
                    SearchableName = FundingController.SanitiseName(organisation.Name),
                    Identifiers = identifiers
                };

                var periodValue = Convert.ToInt64(GetDataFromMillions(spreadsheet, 1, organisation.SpreadsheetRowNumber, 11) / 25.0);
                var periods = GetPeriods(periodValue);

                var fundingValue = GetFundingValue(spreadsheet, organisation, periods);

                var primaryId = organisation.Type == Type.Region || organisation.Type == Type.LocalGovernmentGroup ? organisation.Name :
                    groupingOrg.Identifiers.FirstOrDefault(id => id.Type == OrganisationTypeIdentifier.UKPRN)?.Value;

                var providerFundings = new List<string>();

                switch (organisation.Type)
                {
                    case Type.LA:
                        if ((organisationTypes?.Any() != true || organisationTypes?.Contains(OrganisationGroupTypeIdentifier.LocalAuthorityCode) == true) &&
                            (groupingReasons?.Any() != true || groupingReasons?.Contains(GroupingReason.Payment) == true))
                        {
                            providerFundings = new List<string>
                            {
                                $"{stream.Code}_{period.Period}_{primaryId}_{fundingVersion}"
                            };
                        }

                        break;
                    case Type.Region:
                    case Type.LocalGovernmentGroup:
                        if ((organisationTypes?.Any() != true || organisationTypes?.Contains(OrganisationGroupTypeIdentifier.RegionCode) == true || organisationTypes?.Contains(OrganisationGroupTypeIdentifier.LocalGovernmentGroupCode) == true) &&
                            (groupingReasons?.Any() != true || groupingReasons?.Contains(GroupingReason.Information) == true))
                        {
                            providerFundings.AddRange(GetLasForRegion(organisation.Name).Select(la => $"{stream.Code}_{period.Period}_{la.UKPRN}_{fundingVersion}").ToList());
                        }

                        break;
                }

                if (variationReasons?.Any() == true)
                {
                    providerFundings.Clear();
                }

                var data = new FeedBaseModel
                {
                    SchemaUri = schemaUri,
                    SchemaVersion = schemaVersion,
                    Funding = new FundingFeed
                    {
                        FundingStream = stream,
                        FundingPeriod = period,
                        Status = FundingStatus.Released,
                        GroupingReason = organisation.Type == Type.LA ? GroupingReason.Payment : GroupingReason.Information,
                        FundingVersion = fundingVersion.Replace("-", "."),
                        OrganisationGroup = groupingOrg,
                        FundingValue = fundingValue,
                        ProviderFundings = providerFundings,
                        StatusChangedDate = new DateTimeOffset(new DateTime(2019, 3, 1)),
                        ExternalPublicationDate = new DateTimeOffset(new DateTime(2019, 3, 7)),
                        EarliestPaymentAvailableDate = new DateTimeOffset(new DateTime(2019, 3, 14))
                    }
                };

                if (fundingLineTypes?.Any() == true)
                {
                    // TODO fix after funding model update
                    //foreach (var dperiod in data.Funding.FundingValue.FundingValueByDistributionPeriod)
                    //{
                    //    dperiod.FundingLines = dperiod.FundingLines.Where(line => fundingLineTypes.Contains(line.Type)).ToList();

                    //    //TODO - filter at lower levels
                    //}
                }

                if (templateLineIds?.Any() == true)
                {
                    // TODO fix after funding model update
                    //foreach (var dperiod in data.Funding.FundingValue.FundingValueByDistributionPeriod)
                    //{
                    //    dperiod.FundingLines = dperiod.FundingLines.Where(line => templateLineIds.Contains(line.TemplateLineId.ToString())).ToList();

                    //    //TODO - filter at lower levels
                    //}
                }

                var host = "http://example.org";

                var data0 = new FeedResponseContentModel
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
                };

                totalList.Add(data0);
            }

            return totalList.ToArray();
        }

        /// <summary>
        /// Get the provider funding from the spreadsheet.
        /// </summary>
        /// <param name="id">The id to lookup.</param>
        /// <returns>A provider funding object.</returns>
        public static ProviderFunding GenerateProviderFunding(string id)
        {
            var idParts = id.Split('_');
            var name = idParts[2].Replace("MOCKUKPRN", string.Empty);

            var spreadsheet = GetSpreadsheet();

            var allLas = GetLAs(spreadsheet);
            var las = allLas.Where(item => string.IsNullOrEmpty(name)
                || item.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)
                || item.Code.Equals(name, StringComparison.InvariantCultureIgnoreCase)).ToList();

            var allRegions = GetRegions(spreadsheet);
            var regions = allRegions.Where(item => string.IsNullOrEmpty(name)
                || item.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).ToList();

            var organisations = las.ToList(); // Shallow copy
            organisations.AddRange(regions);

            var organisation = organisations.First();

            var identifiers = new List<ProviderIdentifier>();

            if (organisation.Type == Type.LA)
            {
                identifiers.Add(
                    new ProviderIdentifier
                    {
                        Type = ProviderTypeIdentifier.LACode,
                        Value = organisation.Code
                    }
                );

                identifiers.Add(
                    new ProviderIdentifier
                    {
                        Type = ProviderTypeIdentifier.UKPRN,
                        Value = organisation.UKPRN
                    }
                );
            }

            var ukOffset = new TimeSpan(0, 0, 0);

            var period = new FundingPeriod
            {
                Period = "FY1920",
                Name = "Financial year 2019-20",
                Type = PeriodType.FY,
                StartDate = new DateTimeOffset(2019, 4, 1, 0, 0, 0, ukOffset),
                EndDate = new DateTimeOffset(2020, 3, 31, 0, 0, 0, ukOffset)
            };

            var stream = new FundingStream
            {
                Code = "DSG",
                Name = "Dedicated Schools Grant",
            };

            var periodValue = Convert.ToInt64(GetDataFromMillions(spreadsheet, 1, organisation.SpreadsheetRowNumber, 11) / 25.0);
            var periods = GetPeriods(periodValue);

            var fundingValue = GetFundingValue(spreadsheet, organisation, periods);

            return new ProviderFunding
            {
                FundingVersion = "1.0",
                Provider = new FundingMock.Web.Models.Provider
                {
                    Name = organisation.Name,
                    SearchableName = FundingController.SanitiseName(organisation.Name),
                    ProviderType = organisation.Type.ToString(),
                    ProviderVersionId = "TBC",
                    OtherIdentifiers = identifiers
                },
                FundingPeriodId = period.Period,
                FundingStreamCode = stream.Code,
                FundingValue = fundingValue
            };
        }

        /// <summary>
        /// Get the spreadsheet as a dataset.
        /// </summary>
        /// <returns>A dataset containing 5 data tables.</returns>
        private static DataSet GetSpreadsheet()
        {
            DataSet spreadsheet;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var assembly = Assembly.GetExecutingAssembly();
            var stringBodyPath = assembly.GetManifestResourceNames().Single(str => str.EndsWith("DSG_2019-20_Mar_Tables_Values.xls"));

            using (var fileStream = assembly.GetManifestResourceStream(stringBodyPath))
            {
                using (var reader = ExcelReaderFactory.CreateReader(fileStream))
                {
                    spreadsheet = reader.AsDataSet();
                }
            }

            return spreadsheet;
        }

        /// <summary>
        /// Get the regions from the spreadsheet (on sheet 1).
        /// </summary>
        /// <param name="spreadsheet"></param>
        /// <returns>A list of organisations.</returns>
        private static List<Org> GetLAs(DataSet spreadsheet)
        {
            var returnList = new List<Org>();

            for (var idx = 5; idx < 154; idx++)
            {
                returnList.Add(new Org
                {
                    Code = GetDataString(spreadsheet, 1, idx, 0),
                    Name = GetDataString(spreadsheet, 1, idx, 1),
                    UKPRN = "MOCKUKPRN" + GetDataString(spreadsheet, 1, idx, 0),
                    SpreadsheetRowNumber = idx,
                    Type = Type.LA
                });
            }

            return returnList;
        }

        /// <summary>
        /// Get the regions from the spreadsheet (on sheet 1).
        /// </summary>
        /// <param name="spreadsheet"></param>
        /// <returns>A list of organisations.</returns>
        private static List<Org> GetRegions(DataSet spreadsheet)
        {
            var returnList = new List<Org>();

            for (var idx = 160; idx < 169; idx++)
            {
                returnList.Add(new Org
                {
                    Name = GetDataString(spreadsheet, 1, idx, 0),
                    SpreadsheetRowNumber = idx,
                    Type = Type.Region
                });
            }

            return returnList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spreadsheet"></param>
        /// <param name="sheetNumber"></param>
        /// <param name="rowIdx"></param>
        /// <param name="columnIdx"></param>
        /// <returns></returns>
        private static string GetDataString(DataSet spreadsheet, int sheetNumber, int rowIdx, int columnIdx)
        {
            var table = spreadsheet.Tables[sheetNumber];

            if (table.Rows.Count <= rowIdx && rowIdx >= 155 && rowIdx < 169)
            {
                return "--";
            }

            var valObj = table.Rows[rowIdx]["Column" + columnIdx];

            switch (valObj.GetType().Name)
            {
                case "String":
                    return (string)valObj;
                case "Double":
                    return ((double)valObj).ToString();
            }

            return "--";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spreadsheet"></param>
        /// <param name="sheetNumber"></param>
        /// <param name="rowIdx"></param>
        /// <param name="columnIdx"></param>
        /// <returns></returns>
        private static long GetDataFromMillions(DataSet spreadsheet, int sheetNumber, int rowIdx, int columnIdx)
        {
            var table = spreadsheet.Tables[sheetNumber];

            if (table.Rows.Count <= rowIdx && rowIdx >= 155 && rowIdx < 169)
            {
                return -2;
            }

            var valObj = table.Rows[rowIdx]["Column" + columnIdx];

            switch (valObj.GetType().Name)
            {
                case "Double":
                    var transposed = (double)valObj * 1e8; // 1e8 rather then 1e6 as want in millions of pence, not pounds
                    return Convert.ToInt64(transposed);
            }

            // Some data (all of HN deductions + some rate values for early years) isn't there for regions
            if (rowIdx >= 155 && rowIdx < 169)
            {
                return -2;
            }

            throw new Exception("Can't convert from millions");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spreadsheet"></param>
        /// <param name="sheetNumber"></param>
        /// <param name="rowIdx"></param>
        /// <param name="columnIdx"></param>
        /// <returns></returns>
        private static long GetDataInPence(DataSet spreadsheet, int sheetNumber, int rowIdx, int columnIdx)
        {
            var table = spreadsheet.Tables[sheetNumber];

            if (table.Rows.Count <= rowIdx && rowIdx >= 155 && rowIdx < 169)
            {
                return -2;
            }

            var valObj = table.Rows[rowIdx]["Column" + columnIdx];

            switch (valObj.GetType().Name)
            {
                case "Double":
                    var transposed = (double)valObj * 1e2;
                    return Convert.ToInt64(transposed);
            }

            // Some data (all of HN deductions + some rate values for early years) isn't there for regions
            if (rowIdx >= 155 && rowIdx < 169)
            {
                return -2;
            }

            throw new Exception("Can't convert to pence");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spreadsheet"></param>
        /// <param name="sheetNumber"></param>
        /// <param name="rowIdx"></param>
        /// <param name="columnIdx"></param>
        /// <returns></returns>
        private static long GetData(DataSet spreadsheet, int sheetNumber, int rowIdx, int columnIdx)
        {
            var table = spreadsheet.Tables[sheetNumber];

            if (table.Rows.Count <= rowIdx && rowIdx >= 155 && rowIdx < 169)
            {
                return -2;
            }

            var valObj = table.Rows[rowIdx]["Column" + columnIdx];

            switch (valObj.GetType().Name)
            {
                case "Double":
                    return Convert.ToInt64((double)valObj);
            }

            // Some data (all of HN deductions + some rate values for early years) isn't there for regions
            if (rowIdx >= 155 && rowIdx < 169)
            {
                return -2;
            }

            throw new Exception("Can't read as double");
        }

        /// <summary>
        /// Get the funding value from the spreadshet.
        /// </summary>
        /// <param name="spreadsheet">A dataset representation of the spreadsheet.</param>
        /// <param name="org">The organisation details.</param>
        /// <param name="periods">The periods in a DSG file.</param>
        /// <returns>A funding value object.</returns>
        private static FundingValue GetFundingValue(DataSet spreadsheet, Org org, List<FundingLinePeriod> periods)
        {
            uint templateLineId = 1;
            uint templateCalculationId = 1;
            uint templateReferenceId = 1;

            return new FundingValue
            {
                TotalValue = GetDataFromMillions(spreadsheet, 1, org.SpreadsheetRowNumber, 11),
                FundingLines = new List<FundingLine>
                        {
                            new FundingLine
                            {
                                Name = "PriorToRecoupment",
                                TemplateLineId = templateLineId++,
                                Type = FundingLineType.Information,
                                Value = GetDataFromMillions(spreadsheet, 1, org.SpreadsheetRowNumber, 7),
                                FundingLines = new List<FundingLine>
                                {
                                    new FundingLine
                                    {
                                        Name = "School Block",
                                        TemplateLineId = templateLineId++,
                                        Type = FundingLineType.Information,
                                        Value = GetDataFromMillions(spreadsheet, 1, org.SpreadsheetRowNumber, 3),
                                        FundingLines = new List<FundingLine>
                                        {
                                            new FundingLine
                                            {
                                                Name = "Funding through the mobility and premises factors",
                                                TemplateLineId = templateLineId++,
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "2019-20 mobility and premises funding",
                                                        Type = CalculationType.LumpSum,
                                                        Value = GetDataFromMillions(spreadsheet, 2, org.SpreadsheetRowNumber, 7),
                                                        ValueFormat = CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "2019-20 funding through the premises and mobility factors",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataFromMillions(spreadsheet, 2, org.SpreadsheetRowNumber, 7),
                                                            }
                                                        }
                                                    }
                                                }
                                            },
                                            new FundingLine
                                            {
                                                Name = "Growth funding",
                                                TemplateLineId = templateLineId++,
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "2019-20 growth funding",
                                                        Type = CalculationType.LumpSum,
                                                        Value = GetDataFromMillions(spreadsheet, 2, org.SpreadsheetRowNumber, 8),
                                                        ValueFormat = CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "2019-20 schools block primary unit of funding",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataFromMillions(spreadsheet, 2, org.SpreadsheetRowNumber, 8)
                                                            }
                                                        }
                                                    }
                                                }
                                            },
                                            new FundingLine
                                            {
                                                Name = "Pupil Led funding",
                                                TemplateLineId = templateLineId++,
                                                Value = (GetData(spreadsheet, 2, org.SpreadsheetRowNumber, 4) * GetDataInPence(spreadsheet, 2, org.SpreadsheetRowNumber, 2)) +
                                                    GetData(spreadsheet, 2, org.SpreadsheetRowNumber, 5) * GetDataInPence(spreadsheet, 2, org.SpreadsheetRowNumber, 3),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "Primary Pupil funding",
                                                        Type = CalculationType.PerPupilFunding,
                                                        Value = GetData(spreadsheet, 2, org.SpreadsheetRowNumber, 4) * GetDataInPence(spreadsheet, 2, org.SpreadsheetRowNumber, 2),
                                                        ValueFormat = CalculationValueFormat.Currency,
                                                        FormulaText = "= RefData1 * RefData2",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "2019-20 schools block primary pupils (headcount)",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetData(spreadsheet, 2, org.SpreadsheetRowNumber, 4)
                                                            },
                                                            new ReferenceData
                                                            {
                                                                Name = "2019-20 schools block primary unit of funding",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataInPence(spreadsheet, 2, org.SpreadsheetRowNumber, 2)
                                                            }
                                                        }
                                                    },
                                                    new Calculation
                                                    {
                                                        Name = "Secondary Pupil funding",
                                                        Type = CalculationType.PerPupilFunding,
                                                        Value = GetData(spreadsheet, 2, org.SpreadsheetRowNumber, 5) * GetDataInPence(spreadsheet, 2, org.SpreadsheetRowNumber, 3),
                                                        ValueFormat = CalculationValueFormat.Currency,
                                                        FormulaText = "= RefData1 * RefData2",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "2019-20 schools block secondary pupils (headcount)",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetData(spreadsheet, 2, org.SpreadsheetRowNumber, 5)
                                                            },
                                                            new ReferenceData
                                                            {
                                                                Name = "2019-20 schools block secondary unit of funding",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataInPence(spreadsheet, 2, org.SpreadsheetRowNumber, 3)
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    },
                                    new FundingLine
                                    {
                                        Name = "Central School Services Block",
                                        TemplateLineId = templateLineId++,
                                        Type = FundingLineType.Information,
                                        Value = GetDataFromMillions(spreadsheet, 1, org.SpreadsheetRowNumber, 4),
                                        FundingLines = new List<FundingLine>
                                        {
                                            new FundingLine
                                            {
                                                Name = "CSSB Pupil Led Funding",
                                                Value = GetData(spreadsheet, 2, org.SpreadsheetRowNumber, 12) * GetDataInPence(spreadsheet, 2, org.SpreadsheetRowNumber, 11),
                                                TemplateLineId = templateLineId++,
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "CSSB Pupil funding",
                                                        Type = CalculationType.PerPupilFunding,
                                                        Value = GetData(spreadsheet, 2, org.SpreadsheetRowNumber, 12) * GetDataInPence(spreadsheet, 2, org.SpreadsheetRowNumber, 11),
                                                        ValueFormat = CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "2019-20 CSSB pupils (headcount)",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetData(spreadsheet, 2, org.SpreadsheetRowNumber, 12)
                                                            },
                                                            new ReferenceData
                                                            {
                                                                Name = "2019-20 CSSB unit of funding (£s)",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataInPence(spreadsheet, 2, org.SpreadsheetRowNumber, 11)
                                                            }
                                                        }
                                                    }
                                                }
                                            },
                                            new FundingLine
                                            {
                                                Name = "Funding for historic commitments",
                                                TemplateLineId = templateLineId++,
                                                Value = GetDataFromMillions(spreadsheet, 2, org.SpreadsheetRowNumber, 13),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "2019-20 CSSB funding for historic commitments",
                                                        Type = CalculationType.LumpSum,
                                                        Value = GetDataFromMillions(spreadsheet, 2, org.SpreadsheetRowNumber, 13),
                                                        ValueFormat = CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "2019-20 CSSB funding for historic commitments",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataFromMillions(spreadsheet, 2, org.SpreadsheetRowNumber, 13)
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    },
                                    new FundingLine
                                    {
                                        Name = "High Needs block funding",
                                        TemplateLineId = templateLineId++,
                                        Type = FundingLineType.Information,
                                        Value = GetDataFromMillions(spreadsheet, 1, org.SpreadsheetRowNumber, 5),
                                        FundingLines = new List<FundingLine>
                                        {
                                            new FundingLine
                                            {
                                                Name = "Actual high needs fundings, excluding basic entitelment factor and import/export adjustments",
                                                TemplateLineId = templateLineId++,
                                                Value = GetDataFromMillions(spreadsheet, 3, org.SpreadsheetRowNumber, 2),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "HN Block Before Basic Entitlement and Import and Export Adjustments",
                                                        Type = CalculationType.LumpSum,
                                                        Value = GetDataFromMillions(spreadsheet, 3, org.SpreadsheetRowNumber, 2),
                                                        ValueFormat = CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "HN Block Before Basic Entitlement and Import and Export Adjustments",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataFromMillions(spreadsheet, 3, org.SpreadsheetRowNumber, 2)
                                                            }
                                                        }
                                                    }
                                                }
                                            },
                                            new FundingLine
                                            {
                                                Name = "Basic entitlements",
                                                TemplateLineId = templateLineId++,
                                                Value = GetDataInPence(spreadsheet, 3, org.SpreadsheetRowNumber, 3) * GetData(spreadsheet, 3, org.SpreadsheetRowNumber, 4),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "HN Block Before Basic Entitlement and Import and Export Adjustments",
                                                        Type = CalculationType.LumpSum,
                                                        Value = GetDataInPence(spreadsheet, 3, org.SpreadsheetRowNumber, 3) * GetData(spreadsheet, 3, org.SpreadsheetRowNumber, 4),
                                                        ValueFormat = CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "Basic Entitlement Pupils",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetData(spreadsheet, 3, org.SpreadsheetRowNumber, 4)
                                                            },
                                                            new ReferenceData
                                                            {
                                                                Name = "2019-20 ACA-weighted basic entitlement factor unit rate",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataInPence(spreadsheet, 3, org.SpreadsheetRowNumber, 3)
                                                            }
                                                        }
                                                    }
                                                }
                                            },
                                            new FundingLine
                                            {
                                                Name = "Import / export adjustments",
                                                TemplateLineId = templateLineId++,
                                                Value = GetDataFromMillions(spreadsheet, 3, org.SpreadsheetRowNumber, 5),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "Import/Export Adjustment",
                                                        Type = CalculationType.LumpSum,
                                                        Value = GetDataFromMillions(spreadsheet, 3, org.SpreadsheetRowNumber, 5),
                                                        ValueFormat = CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "Import/Export Adjustment Rate",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataFromMillions(spreadsheet, 3, org.SpreadsheetRowNumber, 5)
                                                            }
                                                        }
                                                    }
                                                }
                                            },
                                            new FundingLine
                                            {
                                                Name = "ONS Population Projection",
                                                TemplateLineId = templateLineId++,
                                                Value = GetData(spreadsheet, 3, org.SpreadsheetRowNumber, 6),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "ONS Population Projection",
                                                        Type = CalculationType.PupilNumber,
                                                        Value = GetData(spreadsheet, 3, org.SpreadsheetRowNumber, 6),
                                                        ValueFormat = CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "Basic Entitlement Pupils",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetData(spreadsheet, 3, org.SpreadsheetRowNumber, 6)
                                                            }
                                                        }
                                                    }
                                                }
                                            },
                                            new FundingLine
                                            {
                                                Name = "Additional High Needs Funding",
                                                TemplateLineId = templateLineId++,
                                                Value = GetDataFromMillions(spreadsheet, 3, org.SpreadsheetRowNumber, 7),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "Additional High Needs Funding",
                                                        Type = CalculationType.LumpSum,
                                                        Value = GetDataFromMillions(spreadsheet, 3, org.SpreadsheetRowNumber, 7),
                                                        ValueFormat = CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "Additional High Needs",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataFromMillions(spreadsheet, 3, org.SpreadsheetRowNumber, 7)
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    },
                                    new FundingLine
                                    {
                                        Name = "Early Years Block",
                                        TemplateLineId = templateLineId++,
                                        Type = FundingLineType.Information,
                                        Value = GetDataFromMillions(spreadsheet, 1, org.SpreadsheetRowNumber, 6),
                                        FundingLines = new List<FundingLine>
                                        {
                                            new FundingLine
                                            {
                                                Name = "Initial funding funding for universal entitlement for 3 and 4 year olds",
                                                TemplateLineId = templateLineId++,
                                                Value = GetData(spreadsheet, 5, org.SpreadsheetRowNumber, 3) *  GetDataInPence(spreadsheet, 5, org.SpreadsheetRowNumber, 2) * 15 * 38,
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "HN Block Before Basic Entitlement and Import and Export Adjustments",
                                                        Type = CalculationType.PerPupilFunding,
                                                        Value = GetData(spreadsheet, 5, org.SpreadsheetRowNumber, 3) *  GetDataInPence(spreadsheet, 5, org.SpreadsheetRowNumber, 2) * 15 * 38,
                                                        ValueFormat = CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "Total 3 and 4 Year Olds (PTE)",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetData(spreadsheet, 5, org.SpreadsheetRowNumber, 3)
                                                            },
                                                            new ReferenceData
                                                            {
                                                                Name = "Early Years Universal Entitlement for 3 and 4 Year Olds Rate",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataInPence(spreadsheet, 5, org.SpreadsheetRowNumber, 2)
                                                            },
                                                            new ReferenceData
                                                            {
                                                                Name = "PTE Funded Hours",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = 15
                                                            }
                                                        }
                                                    }
                                                }
                                            },
                                            new FundingLine
                                            {
                                                Name = "Initial funding funding for additional 15 hours entitlement for eligible working parents of 3 and 4 year olds",
                                                TemplateLineId = templateLineId++,
                                                Value = GetData(spreadsheet, 5, org.SpreadsheetRowNumber, 5) *  GetDataInPence(spreadsheet, 5, org.SpreadsheetRowNumber, 2) * 15 * 38,
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "HN Block Before Basic Entitlement and Import and Export Adjustment",
                                                        Type = CalculationType.PerPupilFunding,
                                                        Value = GetData(spreadsheet, 5, org.SpreadsheetRowNumber, 5) *  GetDataInPence(spreadsheet, 5, org.SpreadsheetRowNumber, 2) * 15 * 38,
                                                        ValueFormat = CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "Total 3 and 4 Year Old for Additional Hours for Working Parents (PTE)",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetData(spreadsheet, 5, org.SpreadsheetRowNumber, 5)
                                                            },
                                                            new ReferenceData
                                                            {
                                                                Name = "Early Years Universal Entitlement for 3 and 4 Year Olds Rate",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataInPence(spreadsheet, 5, org.SpreadsheetRowNumber, 2)
                                                            },
                                                            new ReferenceData
                                                            {
                                                                Name = "PTE Funded Hours",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = 15
                                                            }
                                                        }
                                                    }
                                                }
                                            },
                                            new FundingLine
                                            {
                                                Name = "Initial funding funding for 2 year old entitlement",
                                                TemplateLineId = templateLineId++,
                                                Value = GetData(spreadsheet, 5, org.SpreadsheetRowNumber, 8) *  GetDataInPence(spreadsheet, 5, org.SpreadsheetRowNumber, 7) * 15 * 38,
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "Per Pupil Funding for 2 year old entitlement",
                                                        Type = CalculationType.PerPupilFunding,
                                                        Value = GetData(spreadsheet, 5, org.SpreadsheetRowNumber, 8) *  GetDataInPence(spreadsheet, 5, org.SpreadsheetRowNumber, 7) * 15 * 38,
                                                        ValueFormat = CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "Total 2 Year Olds (PTE)",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetData(spreadsheet, 5, org.SpreadsheetRowNumber, 8)
                                                            },
                                                            new ReferenceData
                                                            {
                                                                Name = "LA hourly rate for 2 year old entitlement",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataInPence(spreadsheet, 5, org.SpreadsheetRowNumber, 7)
                                                            },
                                                            new ReferenceData
                                                            {
                                                                Name = "PTE Funded Hours",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = 15
                                                            }
                                                        }
                                                    }
                                                }
                                            },
                                            new FundingLine
                                            {
                                                Name = "Early Years Pupil Premium",
                                                TemplateLineId = templateLineId++,
                                                Value = GetDataFromMillions(spreadsheet, 2, org.SpreadsheetRowNumber, 10),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "Early Years Pupil Premium lumpsum",
                                                        Type = CalculationType.LumpSum,
                                                        Value = GetDataFromMillions(spreadsheet, 2, org.SpreadsheetRowNumber, 10),
                                                        ValueFormat = CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "Early Years Pupil Premium Rate",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = -1 // Not in the file
                                                            }
                                                        }
                                                    }
                                                }
                                            },
                                            new FundingLine
                                            {
                                                Name = "Disability Access Fund",
                                                TemplateLineId = templateLineId++,
                                                Value = GetDataFromMillions(spreadsheet, 2, org.SpreadsheetRowNumber, 11),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "Disability Access Fund lumpsum",
                                                        Type = CalculationType.LumpSum,
                                                        Value = GetDataFromMillions(spreadsheet, 2, org.SpreadsheetRowNumber, 11),
                                                        ValueFormat = CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "Disability Access Fund Rate",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = -1 // Not in the file
                                                            }
                                                        }
                                                    }
                                                }
                                            },
                                            new FundingLine
                                            {
                                                Name = "Maintained Nursery School Supplementary funding",
                                                TemplateLineId = templateLineId++,
                                                Value = GetDataFromMillions(spreadsheet, 2, org.SpreadsheetRowNumber, 12),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "Per Pupil Funding for Maintained Nursery Schools",
                                                        Type = CalculationType.PerPupilFunding,
                                                        Value = GetDataFromMillions(spreadsheet, 2, org.SpreadsheetRowNumber, 12),
                                                        ValueFormat = CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "Maintained Nursery Schools Supplement (PTE)",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = -1 // Not in the file
                                                            },
                                                            new ReferenceData
                                                            {
                                                                Name = "Maintained Nursery Schools Supplement Hourly Rate",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = -1 // Not in the file
                                                            },
                                                            new ReferenceData
                                                            {
                                                                Name = "PTE Funded Hours",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = -1 // Not in the file
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            },
                            new FundingLine
                            {
                                Name = "PostDeductionForRecoupmentAndHighNeeds",
                                FundingLineCode = "DSG-001",
                                TemplateLineId = templateLineId++,
                                Type = FundingLineType.Payment,
                                Value = GetDataFromMillions(spreadsheet, 1, org.SpreadsheetRowNumber, 11),
                                FundingLines = new List<FundingLine>
                                {
                                    new FundingLine
                                    {
                                        Name = "School Block",
                                        TemplateLineId = templateLineId++,
                                        Type = FundingLineType.Information,
                                        Value = GetDataFromMillions(spreadsheet, 1, org.SpreadsheetRowNumber, 7),
                                        FundingLines = new List<FundingLine>
                                        {
                                            new FundingLine
                                            {
                                                Name = "Funding through the mobility and premises factors",
                                                TemplateLineId = templateLineId++,
                                                Value = GetDataFromMillions(spreadsheet, 2, org.SpreadsheetRowNumber, 6),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "2019-20 mobility and premises funding",
                                                        Type = CalculationType.LumpSum,
                                                        Value = GetDataFromMillions(spreadsheet, 2, org.SpreadsheetRowNumber, 6),
                                                        ValueFormat = CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "2019-20 funding through the premises and mobility factors",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataFromMillions(spreadsheet, 2, org.SpreadsheetRowNumber, 6)
                                                            }
                                                        }
                                                    }
                                                }
                                            },
                                            new FundingLine
                                            {
                                                Name = "Growth funding",
                                                TemplateLineId = templateLineId++,
                                                Value = GetDataFromMillions(spreadsheet, 2, org.SpreadsheetRowNumber, 7),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "2019-20 growth funding",
                                                        Type = CalculationType.LumpSum,
                                                        Value = GetDataFromMillions(spreadsheet, 2, org.SpreadsheetRowNumber, 7),
                                                        ValueFormat = CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "2019-20 schools block primary unit of funding",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataFromMillions(spreadsheet, 2, org.SpreadsheetRowNumber, 7)
                                                            }
                                                        }
                                                    }
                                                }
                                            },
                                            new FundingLine
                                            {
                                                Name = "Pupil Led funding",
                                                TemplateLineId = templateLineId++,
                                                Value = GetDataFromMillions(spreadsheet, 2, org.SpreadsheetRowNumber, 2) * GetData(spreadsheet, 2, org.SpreadsheetRowNumber, 4),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "2019-20 schools block primary unit of funding",
                                                        Type = CalculationType.PerPupilFunding,
                                                        Value = GetDataFromMillions(spreadsheet, 2, org.SpreadsheetRowNumber, 2) * GetData(spreadsheet, 2, org.SpreadsheetRowNumber, 4),
                                                        ValueFormat = CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "2019-20 schools block primary pupils (headcount)",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetData(spreadsheet, 2, org.SpreadsheetRowNumber, 4)
                                                            },
                                                            new ReferenceData
                                                            {
                                                                Name = "2019-20 schools block primary unit of funding",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataFromMillions(spreadsheet, 2, org.SpreadsheetRowNumber, 2)
                                                            }
                                                        }
                                                    }
                                                }
                                            },
                                            new FundingLine
                                            {
                                                Name = "Deductions To School Block",
                                                TemplateLineId = templateLineId++,
                                                Value = GetDataFromMillions(spreadsheet, 2, org.SpreadsheetRowNumber, 9),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "Deductions To School Block",
                                                        Type = CalculationType.LumpSum,
                                                        Value =  GetDataFromMillions(spreadsheet, 2, org.SpreadsheetRowNumber, 9),
                                                        ValueFormat = CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "Rate Deductions To School Block",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = -1 // Not given in file
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    },
                                    new FundingLine
                                    {
                                        Name = "Central School Services Block",
                                        TemplateLineId = templateLineId++,
                                        Type = FundingLineType.Information,
                                        Value = GetDataFromMillions(spreadsheet, 1, org.SpreadsheetRowNumber, 8),
                                        FundingLines = new List<FundingLine>
                                        {
                                            new FundingLine
                                            {
                                                Name = "CSSB Pupil Led Funding",
                                                TemplateLineId = templateLineId++,
                                                Value = GetDataInPence(spreadsheet, 2, org.SpreadsheetRowNumber, 11) * GetData(spreadsheet, 2, org.SpreadsheetRowNumber, 12),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "Rate Deductions To School Block",
                                                        Type = CalculationType.PerPupilFunding,
                                                        Value = GetDataInPence(spreadsheet, 2, org.SpreadsheetRowNumber, 11) * GetData(spreadsheet, 2, org.SpreadsheetRowNumber, 12),
                                                        ValueFormat = CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "2019-20 CSSB pupils (headcount)",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetData(spreadsheet, 2, org.SpreadsheetRowNumber, 12)
                                                            },
                                                            new ReferenceData
                                                            {
                                                                Name = "2019-20 CSSB unit of funding (£s)",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataInPence(spreadsheet, 2, org.SpreadsheetRowNumber, 11)
                                                            }
                                                        }
                                                    }
                                                }
                                            },
                                            new FundingLine
                                            {
                                                Name = "Funding for historic commitments",
                                                Value = GetDataFromMillions(spreadsheet, 2, org.SpreadsheetRowNumber, 13),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "2019-20 CSSB funding for historic commitments",
                                                        Type = CalculationType.LumpSum,
                                                        Value = GetDataFromMillions(spreadsheet, 2, org.SpreadsheetRowNumber, 13),
                                                        ValueFormat = CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "2019-20 CSSB funding for historic commitments",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataFromMillions(spreadsheet, 2, org.SpreadsheetRowNumber, 13)
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    },
                                    new FundingLine
                                    {
                                        Name = "High Needs block funding",
                                        TemplateLineId = 0,
                                        Type = FundingLineType.Information,
                                        Value = GetDataFromMillions(spreadsheet, 1, org.SpreadsheetRowNumber, 9),
                                        FundingLines = new List<FundingLine>
                                        {
                                            new FundingLine
                                            {
                                                Name = "Actual high needs fundings, excluding basic entitelment factor and import/export adjustments",
                                                Value = GetDataFromMillions(spreadsheet, 3, org.SpreadsheetRowNumber, 2),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "HN Block Before Basic Entitlement and Import and Export Adjustments",
                                                        Type = CalculationType.LumpSum,
                                                        Value = GetDataFromMillions(spreadsheet, 3, org.SpreadsheetRowNumber, 2),
                                                        ValueFormat = CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "HN Block Before Basic Entitlement and Import and Export Adjustments",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataFromMillions(spreadsheet, 3, org.SpreadsheetRowNumber, 2)
                                                            }
                                                        }
                                                    }
                                                }
                                            },
                                            new FundingLine
                                            {
                                                Name = "Basic entitlements",
                                                Value = GetData(spreadsheet, 3, org.SpreadsheetRowNumber, 4) * GetDataInPence(spreadsheet, 3, org.SpreadsheetRowNumber, 3),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "HN Block Before Basic Entitlement and Import and Export Adjustments",
                                                        Type = CalculationType.PerPupilFunding,
                                                        Value = GetData(spreadsheet, 3, org.SpreadsheetRowNumber, 4) * GetDataInPence(spreadsheet, 3, org.SpreadsheetRowNumber, 3),
                                                        ValueFormat = CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "Basic Entitlement Pupils",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetData(spreadsheet, 3, org.SpreadsheetRowNumber, 4)
                                                            },
                                                            new ReferenceData
                                                            {
                                                                Name = "2019-20 ACA-weighted basic entitlement factor unit rate",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataInPence(spreadsheet, 3, org.SpreadsheetRowNumber, 3)
                                                            }
                                                        }
                                                    }
                                                }
                                            },
                                            new FundingLine
                                            {
                                                Name = "Import / export adjustments",
                                                Value = GetDataFromMillions(spreadsheet, 3, org.SpreadsheetRowNumber, 5),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "Import/Export Adjustment",
                                                        Type = CalculationType.LumpSum,
                                                        Value = GetDataFromMillions(spreadsheet, 3, org.SpreadsheetRowNumber, 5),
                                                        ValueFormat = CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "Import/Export Adjustment Rate",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = -1 // Not given in file
                                                            }
                                                        }
                                                    }
                                                }
                                            },
                                            new FundingLine
                                            {
                                                Name = "ONS Population Projection",
                                                Value = GetData(spreadsheet, 3, org.SpreadsheetRowNumber, 6),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "ONS Population Projection",
                                                        Type = CalculationType.PupilNumber,
                                                        Value = GetData(spreadsheet, 3, org.SpreadsheetRowNumber, 6),
                                                        ValueFormat = CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "Basic Entitlement Pupils",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetData(spreadsheet, 3, org.SpreadsheetRowNumber, 6),
                                                            }
                                                        }
                                                    }
                                                }
                                            },
                                            new FundingLine
                                            {
                                                Name = "Additional High Needs Funding",
                                                Value = GetDataFromMillions(spreadsheet, 3, org.SpreadsheetRowNumber, 7),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "Additional High Needs Funding",
                                                        Type = CalculationType.LumpSum,
                                                        Value = GetDataFromMillions(spreadsheet, 3, org.SpreadsheetRowNumber, 7),
                                                        ValueFormat = CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "Additional High Needs",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataFromMillions(spreadsheet, 3, org.SpreadsheetRowNumber, 7)
                                                            }
                                                        }
                                                    }
                                                }
                                            },
                                            new FundingLine
                                            {
                                                Name = "Total deduction to 2019-19 High Needs Block for direct funding of places by ESFA",
                                                FundingLines = new List<FundingLine>
                                                {
                                                    new FundingLine
                                                    {
                                                        Name = "Mainstream Academies & Free Schools (SEN units and Resourced provision)",
                                                        Value = GetDataFromMillions(spreadsheet, 4, org.SpreadsheetRowNumber, 4) + GetDataFromMillions(spreadsheet, 4, org.SpreadsheetRowNumber, 7)
                                                            + GetDataFromMillions(spreadsheet, 4, org.SpreadsheetRowNumber, 10) + GetDataFromMillions(spreadsheet, 4, org.SpreadsheetRowNumber, 13),
                                                        FundingLines = new List<FundingLine>
                                                        {
                                                            new FundingLine
                                                            {
                                                                Name = "Pre-16 SEN Places @ £6k",
                                                                Value = GetDataFromMillions(spreadsheet, 4, org.SpreadsheetRowNumber, 4),
                                                                Calculations = new List<Calculation>
                                                                {
                                                                    new Calculation
                                                                    {
                                                                        Name = "April 2019-August 2019",
                                                                        Type = CalculationType.PerPupilFunding,
                                                                        Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 2) * 600000,
                                                                        ValueFormat = CalculationValueFormat.Currency,
                                                                        FormulaText = "",
                                                                        TemplateCalculationId = templateCalculationId++,
                                                                        ReferenceData = new List<ReferenceData>
                                                                        {
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "April 2019-August 2020 Pupil Number",
                                                                                Format = ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 2)
                                                                            },
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "April 2019-August 2020 Rate SEN/AP",
                                                                                Format = ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = 600000
                                                                            }
                                                                        }
                                                                    },
                                                                    new Calculation
                                                                    {
                                                                        Name = "Sept 2019-March 2020",
                                                                        Type = CalculationType.PerPupilFunding,
                                                                        Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 3) * 600000,
                                                                        ValueFormat = CalculationValueFormat.Currency,
                                                                        FormulaText = "",
                                                                        TemplateCalculationId = templateCalculationId++,
                                                                        ReferenceData = new List<ReferenceData>
                                                                        {
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "Sept 2019-March 2020 Pupil Number",
                                                                                Format = ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 3)
                                                                            },
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "Sept 2019-March 2020 Rate SEN/AP",
                                                                                Format = ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = 600000
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            },
                                                            new FundingLine
                                                            {
                                                                Name = "Pre-16 SEN Places @ £10k",
                                                                Value = GetDataFromMillions(spreadsheet, 4, org.SpreadsheetRowNumber, 7),
                                                                Calculations = new List<Calculation>
                                                                {
                                                                    new Calculation
                                                                    {
                                                                        Name = "April 2019-August 2019",
                                                                        Type = CalculationType.PerPupilFunding,
                                                                        Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 5) * 1000000,
                                                                        ValueFormat = CalculationValueFormat.Currency,
                                                                        FormulaText = "",
                                                                        TemplateCalculationId = templateCalculationId++,
                                                                        ReferenceData = new List<ReferenceData>
                                                                        {
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "April 2019-August 2020 Pupil Number",
                                                                                Format = ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 5)
                                                                            },
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "April 2019-August 2020 Rate SEN/AP",
                                                                                Format = ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = 1000000
                                                                            }
                                                                        }
                                                                    },
                                                                    new Calculation
                                                                    {
                                                                        Name = "Sept 2019-March 2020",
                                                                        Type = CalculationType.PerPupilFunding,
                                                                        Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 6) * 1000000,
                                                                        ValueFormat = CalculationValueFormat.Currency,
                                                                        FormulaText = "",
                                                                        TemplateCalculationId = templateCalculationId++,
                                                                        ReferenceData = new List<ReferenceData>
                                                                        {
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "Sept 2019-March 2020 Pupil Number",
                                                                                Format = ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 6)
                                                                            },
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "Sept 2019-March 2020 Rate SEN/AP",
                                                                                Format = ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = 1000000
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            },
                                                            new FundingLine
                                                            {
                                                                Name = "Post 16 SEN Place",
                                                                Value = GetDataFromMillions(spreadsheet, 4, org.SpreadsheetRowNumber, 10),
                                                                Calculations = new List<Calculation>
                                                                {
                                                                    new Calculation
                                                                    {
                                                                        Name = "April 2019-August 2019",
                                                                        Type = CalculationType.PerPupilFunding,
                                                                        Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 8) * 240000,
                                                                        ValueFormat = CalculationValueFormat.Currency,
                                                                        FormulaText = "",
                                                                        TemplateCalculationId = templateCalculationId++,
                                                                        ReferenceData = new List<ReferenceData>
                                                                        {
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "April 2019-August 2020 Pupil Number",
                                                                                Format = ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 8)
                                                                            },
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "April 2019-August 2020 Rate SEN/AP",
                                                                                Format = ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = 240000 // I think
                                                                            }
                                                                        }
                                                                    },
                                                                    new Calculation
                                                                    {
                                                                        Name = "Sept 2019-March 2020",
                                                                        Type = CalculationType.PerPupilFunding,
                                                                        Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 9) * 240000,
                                                                        ValueFormat = CalculationValueFormat.Currency,
                                                                        FormulaText = "",
                                                                        TemplateCalculationId = templateCalculationId++,
                                                                        ReferenceData = new List<ReferenceData>
                                                                        {
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "Sept 2019-March 2020 Pupil Number",
                                                                                Format = ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 9)
                                                                            },
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "Sept 2019-March 2020 Rate SEN/AP",
                                                                                Format = ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = 240000 // I think
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            },
                                                            new FundingLine
                                                            {
                                                                Name = "Pre-16 AP Places",
                                                                Value = GetDataFromMillions(spreadsheet, 4, org.SpreadsheetRowNumber, 13),
                                                                Calculations = new List<Calculation>
                                                                {
                                                                    new Calculation
                                                                    {
                                                                        Name = "April 2019-August 2019",
                                                                        Type = CalculationType.PerPupilFunding,
                                                                        Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 11) * -1,
                                                                        ValueFormat = CalculationValueFormat.Currency,
                                                                        FormulaText = "",
                                                                        TemplateCalculationId = templateCalculationId++,
                                                                        ReferenceData = new List<ReferenceData>
                                                                        {
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "April 2019-August 2020 Pupil Number",
                                                                                Format = ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 11)
                                                                            },
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "April 2019-August 2020 Rate SEN/AP",
                                                                                Format = ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = -1 // Not given in file, and I can't infer
                                                                            }
                                                                        }
                                                                    },
                                                                    new Calculation
                                                                    {
                                                                        Name = "Sept 2019-March 2020",
                                                                        Type = CalculationType.PerPupilFunding,
                                                                        Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 12) * -1,
                                                                        ValueFormat = CalculationValueFormat.Currency,
                                                                        FormulaText = "",
                                                                        TemplateCalculationId = templateCalculationId++,
                                                                        ReferenceData = new List<ReferenceData>
                                                                        {
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "Sept 2019-March 2020 Pupil Number",
                                                                                Format = ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 12)
                                                                            },
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "Sept 2019-March 2020 Rate SEN/AP",
                                                                                Format = ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = -1 // Not given in file, and I can't infer
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    },
                                                    new FundingLine
                                                    {
                                                        Name = "Special Academies & Free Schools",
                                                        Value = GetDataFromMillions(spreadsheet, 4, org.SpreadsheetRowNumber, 16) + GetDataFromMillions(spreadsheet, 4, org.SpreadsheetRowNumber, 19)
                                                            + GetDataFromMillions(spreadsheet, 4, org.SpreadsheetRowNumber, 22),
                                                        FundingLines = new List<FundingLine>
                                                        {
                                                            new FundingLine
                                                            {
                                                                Name = "Pre-16 SEN Places",
                                                                Value = GetDataFromMillions(spreadsheet, 4, org.SpreadsheetRowNumber, 16),
                                                                Calculations = new List<Calculation>
                                                                {
                                                                    new Calculation
                                                                    {
                                                                        Name = "April 2019-August 2019",
                                                                        Type = CalculationType.PerPupilFunding,
                                                                        Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 14) * -1,
                                                                        ValueFormat = CalculationValueFormat.Currency,
                                                                        FormulaText = "",
                                                                        TemplateCalculationId = templateCalculationId++,
                                                                        ReferenceData = new List<ReferenceData>
                                                                        {
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "April 2019-August 2020 Pupil Number",
                                                                                Format = ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 14)
                                                                            },
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "April 2019-August 2020 Rate SEN/AP",
                                                                                Format = ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = -1 // Not given
                                                                            }
                                                                        }
                                                                    },
                                                                    new Calculation
                                                                    {
                                                                        Name = "Sept 2019-March 2020",
                                                                        Type = CalculationType.PerPupilFunding,
                                                                        Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 15) * -1,
                                                                        ValueFormat = CalculationValueFormat.Currency,
                                                                        FormulaText = "",
                                                                        TemplateCalculationId = templateCalculationId++,
                                                                        ReferenceData = new List<ReferenceData>
                                                                        {
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "Sept 2019-March 2020 Pupil Number",
                                                                                Format = ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 15)
                                                                            },
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "Sept 2019-March 2020 Rate SEN/AP",
                                                                                Format = ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = -1 // Not given
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            },
                                                            new FundingLine
                                                            {
                                                                Name = "Post 16 SEN Places",
                                                                Value = GetDataFromMillions(spreadsheet, 4, org.SpreadsheetRowNumber, 19),
                                                                Calculations = new List<Calculation>
                                                                {
                                                                    new Calculation
                                                                    {
                                                                        Name = "April 2019-August 2019",
                                                                        Type = CalculationType.PerPupilFunding,
                                                                        Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 17) * -1,
                                                                        ValueFormat = CalculationValueFormat.Currency,
                                                                        FormulaText = "",
                                                                        TemplateCalculationId = templateCalculationId++,
                                                                        ReferenceData = new List<ReferenceData>
                                                                        {
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "April 2019-August 2020 Pupil Number",
                                                                                Format = ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 17)
                                                                            },
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "April 2019-August 2020 Rate SEN/AP",
                                                                                Format = ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = -1 // Not given
                                                                            }

                                                                        }
                                                                    },
                                                                    new Calculation
                                                                    {
                                                                        Name = "Sept 2019-March 2020",
                                                                        Type = CalculationType.PerPupilFunding,
                                                                        Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 18) * -1,
                                                                        ValueFormat = CalculationValueFormat.Currency,
                                                                        FormulaText = "",
                                                                        TemplateCalculationId = templateCalculationId++,
                                                                        ReferenceData = new List<ReferenceData>
                                                                        {
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "Sept 2019-March 2020 Pupil Number",
                                                                                Format = ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 18)
                                                                            },
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "Sept 2019-March 2020 Rate SEN/AP",
                                                                                Format = ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = -1 // Not given
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            },
                                                            new FundingLine
                                                            {
                                                                Name = "Pre-16 AP Place",
                                                                Value = GetDataFromMillions(spreadsheet, 4, org.SpreadsheetRowNumber, 22),
                                                                Calculations = new List<Calculation>
                                                                {
                                                                    new Calculation
                                                                    {
                                                                        Name = "April 2019-August 2019",
                                                                        Type = CalculationType.PerPupilFunding,
                                                                        Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 20) * -1,
                                                                        ValueFormat = CalculationValueFormat.Currency,
                                                                        FormulaText = "",
                                                                        TemplateCalculationId = templateCalculationId++,
                                                                        ReferenceData = new List<ReferenceData>
                                                                        {
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "April 2019-August 2020 Pupil Number",
                                                                                Format = ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 20)
                                                                            },
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "April 2019-August 2020 Rate SEN/AP",
                                                                                Format = ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = -1 // Not given
                                                                            }
                                                                        }
                                                                    },
                                                                    new Calculation
                                                                    {
                                                                        Name = "Sept 2019-March 2020",
                                                                        Type = CalculationType.PerPupilFunding,
                                                                        Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 21) * -1,
                                                                        ValueFormat = CalculationValueFormat.Currency,
                                                                        FormulaText = "",
                                                                        TemplateCalculationId = templateCalculationId++,
                                                                        ReferenceData = new List<ReferenceData>
                                                                        {
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "Sept 2019-March 2020 Pupil Number",
                                                                                Format = ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 21)
                                                                            },
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "Sept 2019-March 2020 Rate SEN/AP",
                                                                                Format = ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = -1 // Not given
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    },
                                                    new FundingLine
                                                    {
                                                        Name = "AP Academies & Free schools",
                                                        Value = GetDataFromMillions(spreadsheet, 4, org.SpreadsheetRowNumber, 25) + GetDataFromMillions(spreadsheet, 4, org.SpreadsheetRowNumber, 28),
                                                        FundingLines = new List<FundingLine>
                                                        {
                                                            new FundingLine
                                                            {
                                                                Name = "Pre-16 SEN Places",
                                                                Value = GetDataFromMillions(spreadsheet, 4, org.SpreadsheetRowNumber, 25),
                                                                Calculations = new List<Calculation>
                                                                {
                                                                    new Calculation
                                                                    {
                                                                        Name = "April 2019-August 2019",
                                                                        Type = CalculationType.PerPupilFunding,
                                                                        Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 23) * -1,
                                                                        ValueFormat = CalculationValueFormat.Currency,
                                                                        FormulaText = "",
                                                                        TemplateCalculationId = templateCalculationId++,
                                                                        ReferenceData = new List<ReferenceData>
                                                                        {
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "April 2019-August 2020 Pupil Number",
                                                                                Format = ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 23)
                                                                            },
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "April 2019-August 2020 Rate SEN/AP",
                                                                                Format = ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = -1 // Not given
                                                                            }
                                                                        }
                                                                    },
                                                                    new Calculation
                                                                    {
                                                                        Name = "Sept 2019-March 2020",
                                                                        Type = CalculationType.PerPupilFunding,
                                                                        Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 24) * -1,
                                                                        ValueFormat = CalculationValueFormat.Currency,
                                                                        FormulaText = "",
                                                                        TemplateCalculationId = templateCalculationId++,
                                                                        ReferenceData = new List<ReferenceData>
                                                                        {
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "Sept 2019-March 2020 Pupil Number",
                                                                                Format = ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 24)
                                                                            },
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "Sept 2019-March 2020 Rate SEN/AP",
                                                                                Format = ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = -1 // Not given
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            },
                                                            new FundingLine
                                                            {
                                                                Name = "Pre-16 AP Places",
                                                                Value = GetDataFromMillions(spreadsheet, 4, org.SpreadsheetRowNumber, 28),
                                                                Calculations = new List<Calculation>
                                                                {
                                                                    new Calculation
                                                                    {
                                                                        Name = "April 2019-August 2019",
                                                                        Type = CalculationType.PerPupilFunding,
                                                                        Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 26) * -1,
                                                                        ValueFormat = CalculationValueFormat.Currency,
                                                                        FormulaText = "",
                                                                        TemplateCalculationId = templateCalculationId++,
                                                                        ReferenceData = new List<ReferenceData>
                                                                        {
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "April 2019-August 2020 Pupil Number",
                                                                                Format = ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 26)
                                                                            },
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "April 2019-August 2020 Rate SEN/AP",
                                                                                Format = ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = -1 // Not given
                                                                            }
                                                                        }
                                                                    },
                                                                    new Calculation
                                                                    {
                                                                        Name = "Sept 2019-March 2020",
                                                                        Type = CalculationType.PerPupilFunding,
                                                                        Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 27) * -1,
                                                                        ValueFormat = CalculationValueFormat.Currency,
                                                                        FormulaText = "",
                                                                        TemplateCalculationId = templateCalculationId++,
                                                                        ReferenceData = new List<ReferenceData>
                                                                        {
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "Sept 2019-March 2020 Pupil Numbe",
                                                                                Format = ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 27)
                                                                            },
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "Sept 2019-March 2020 Rate SEN/AP",
                                                                                Format = ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = -1 // Not given
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    },
                                                    new FundingLine
                                                    {
                                                        Name = "Maintained Special Schools",
                                                        FundingLines = new List<FundingLine>
                                                        {
                                                            new FundingLine
                                                            {
                                                                Name = "Post 16 SEN Places",
                                                                Value = GetDataFromMillions(spreadsheet, 4, org.SpreadsheetRowNumber, 31),
                                                                Calculations = new List<Calculation>
                                                                {
                                                                    new Calculation
                                                                    {
                                                                        Name = "April 2019-August 2019",
                                                                        Type = CalculationType.PerPupilFunding,
                                                                        Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 29) * -1,
                                                                        ValueFormat = CalculationValueFormat.Currency,
                                                                        FormulaText = "",
                                                                        TemplateCalculationId = templateCalculationId++,
                                                                        ReferenceData = new List<ReferenceData>
                                                                        {
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "April 2019-July 2020 Pupil Number",
                                                                                Format = ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 29)
                                                                            },
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "April 2019-July 2020 Rate SEN/AP",
                                                                                Format = ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = -1 // Not given
                                                                            }
                                                                        }
                                                                    },
                                                                    new Calculation
                                                                    {
                                                                        Name = "Sept 2019-March 2020",
                                                                        Type = CalculationType.PerPupilFunding,
                                                                        Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 30) * -1,
                                                                        ValueFormat = CalculationValueFormat.Currency,
                                                                        FormulaText = "",
                                                                        TemplateCalculationId = templateCalculationId++,
                                                                        ReferenceData = new List<ReferenceData>
                                                                        {
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "Aug 2019-March 2020 Pupil Number",
                                                                                Format = ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 30)
                                                                            },
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "Aug 2019-March 2020 Rate SEN/AP",
                                                                                Format = ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = -1 // Not given
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    },
                                                    new FundingLine
                                                    {
                                                        Name = "Maintained Mainstream Schools",
                                                        FundingLines = new List<FundingLine>
                                                        {
                                                            new FundingLine
                                                            {
                                                                Name = "Post 16 SEN Places",
                                                                Value = GetDataFromMillions(spreadsheet, 4, org.SpreadsheetRowNumber, 34),
                                                                Calculations = new List<Calculation>
                                                                {
                                                                    new Calculation
                                                                    {
                                                                        Name = "April 2019-August 2019",
                                                                        Type = CalculationType.PerPupilFunding,
                                                                        Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 32) * -1,
                                                                        ValueFormat = CalculationValueFormat.Currency,
                                                                        FormulaText = "",
                                                                        TemplateCalculationId = templateCalculationId++,
                                                                        ReferenceData = new List<ReferenceData>
                                                                        {
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "April 2019-July 2020 Pupil Number",
                                                                                Format = ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 32)
                                                                            },
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "April 2019-July 2020 Rate SEN/AP",
                                                                                Format = ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = -1 // Not given
                                                                            }
                                                                        }
                                                                    },
                                                                    new Calculation
                                                                    {
                                                                        Name = "Sept 2019-March 2020",
                                                                        Type = CalculationType.PerPupilFunding,
                                                                        Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 33) * -1,
                                                                        ValueFormat = CalculationValueFormat.Currency,
                                                                        FormulaText = "",
                                                                        TemplateCalculationId = templateCalculationId++,
                                                                        ReferenceData = new List<ReferenceData>
                                                                        {
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "Aug 2019-March 2020 Pupil Number",
                                                                                Format = ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 33)
                                                                            },
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "Aug 2019-March 2020 Rate SEN/AP",
                                                                                Format = ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = -1 // Not given
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    },
                                                    new FundingLine
                                                    {
                                                        Name = "Hospital Academies",
                                                        Value = GetDataFromMillions(spreadsheet, 4, org.SpreadsheetRowNumber, 37),
                                                        Calculations = new List<Calculation>
                                                        {
                                                            new Calculation
                                                            {
                                                                Name = "April 2019-August 2019",
                                                                Type = CalculationType.PerPupilFunding,
                                                                Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 35) * -1,
                                                                ValueFormat = CalculationValueFormat.Currency,
                                                                FormulaText = "",
                                                                TemplateCalculationId = templateCalculationId++,
                                                                ReferenceData = new List<ReferenceData>
                                                                {
                                                                    new ReferenceData
                                                                    {
                                                                        Name = "April 2019-July 2020 Pupil Number",
                                                                        Format = ReferenceDataValueFormat.Number,
                                                                        TemplateReferenceId = templateReferenceId++,
                                                                        Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 35)
                                                                    },
                                                                    new ReferenceData
                                                                    {
                                                                        Name = "April 2019-July 2020 Rate SEN/AP",
                                                                        Format = ReferenceDataValueFormat.Number,
                                                                        TemplateReferenceId = templateReferenceId++,
                                                                        Value = -1 // Not given
                                                                    }
                                                                }
                                                            },
                                                            new Calculation
                                                            {
                                                                Name = "Sept 2019-March 2020",
                                                                Type = CalculationType.PerPupilFunding,
                                                                Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 36) * -1,
                                                                ValueFormat = CalculationValueFormat.Currency,
                                                                FormulaText = "",
                                                                TemplateCalculationId = templateCalculationId++,
                                                                ReferenceData = new List<ReferenceData>
                                                                {
                                                                    new ReferenceData
                                                                    {
                                                                        Name = "Aug 2019-March 2020 Pupil Number",
                                                                        Format = ReferenceDataValueFormat.Number,
                                                                        TemplateReferenceId = templateReferenceId++,
                                                                        Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 36)
                                                                    },
                                                                    new ReferenceData
                                                                    {
                                                                        Name = "Aug 2019-March 2020 Rate SEN/AP",
                                                                        Format = ReferenceDataValueFormat.Number,
                                                                        TemplateReferenceId = templateReferenceId++,
                                                                        Value = -1 // Not given
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    },
                                                    new FundingLine
                                                    {
                                                        Name = "16-19 Academies & Free Schools",
                                                        Value = GetDataFromMillions(spreadsheet, 4, org.SpreadsheetRowNumber, 40),
                                                        Calculations = new List<Calculation>
                                                        {
                                                            new Calculation
                                                            {
                                                                Name = "April 2019-August 2019",
                                                                Type = CalculationType.PerPupilFunding,
                                                                Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 38) * -1,
                                                                ValueFormat = CalculationValueFormat.Currency,
                                                                FormulaText = "",
                                                                TemplateCalculationId = templateCalculationId++,
                                                                ReferenceData = new List<ReferenceData>
                                                                {
                                                                    new ReferenceData
                                                                    {
                                                                        Name = "April 2019-July 2020 Pupil Number",
                                                                        Format = ReferenceDataValueFormat.Number,
                                                                        TemplateReferenceId = templateReferenceId++,
                                                                        Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 38)
                                                                    },
                                                                    new ReferenceData
                                                                    {
                                                                        Name = "April 2019-July 2020 Rate SEN/AP",
                                                                        Format = ReferenceDataValueFormat.Number,
                                                                        TemplateReferenceId = templateReferenceId++,
                                                                        Value = -1 // Not given
                                                                    }
                                                                }
                                                            },
                                                            new Calculation
                                                            {
                                                                Name = "Sept 2019-March 2020",
                                                                Type = CalculationType.PerPupilFunding,
                                                                Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 39) * -1,
                                                                ValueFormat = CalculationValueFormat.Currency,
                                                                FormulaText = "",
                                                                TemplateCalculationId = templateCalculationId++,
                                                                ReferenceData = new List<ReferenceData>
                                                                {
                                                                    new ReferenceData
                                                                    {
                                                                        Name = "Aug 2019-March 2020 Pupil Number",
                                                                        Format = ReferenceDataValueFormat.Number,
                                                                        TemplateReferenceId = templateReferenceId++,
                                                                        Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 39)
                                                                    },
                                                                    new ReferenceData
                                                                    {
                                                                        Name = "Aug 2019-March 2020 Rate SEN/AP",
                                                                        Format = ReferenceDataValueFormat.Number,
                                                                        TemplateReferenceId = templateReferenceId++,
                                                                        Value = -1 // Not given
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    },
                                                    new FundingLine
                                                    {
                                                        Name = "FE and ILP",
                                                        Value = GetDataFromMillions(spreadsheet, 4, org.SpreadsheetRowNumber, 43),
                                                        Calculations = new List<Calculation>
                                                        {
                                                            new Calculation
                                                            {
                                                                Name = "April 2019-August 2019",
                                                                Type = CalculationType.LumpSum,
                                                                Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 41) * -1,
                                                                ValueFormat = CalculationValueFormat.Currency,
                                                                FormulaText = "",
                                                                TemplateCalculationId = templateCalculationId++,
                                                                ReferenceData = new List<ReferenceData>
                                                                {
                                                                    new ReferenceData
                                                                    {
                                                                        Name = "April 2019-July 2020 Pupil Number",
                                                                        Format = ReferenceDataValueFormat.Number,
                                                                        TemplateReferenceId = templateReferenceId++,
                                                                        Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 41)
                                                                    },
                                                                    new ReferenceData
                                                                    {
                                                                        Name = "April 2019-July 2020 Rate SEN/AP",
                                                                        Format = ReferenceDataValueFormat.Number,
                                                                        TemplateReferenceId = templateReferenceId++,
                                                                        Value = -1 // Not given
                                                                    }
                                                                }
                                                            },
                                                            new Calculation
                                                            {
                                                                Name = "Sept 2019-March 2020",
                                                                Type = CalculationType.LumpSum,
                                                                Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 42) * -1,
                                                                ValueFormat = CalculationValueFormat.Currency,
                                                                FormulaText = "",
                                                                TemplateCalculationId = templateCalculationId++,
                                                                ReferenceData = new List<ReferenceData>
                                                                {
                                                                    new ReferenceData
                                                                    {
                                                                        Name = "Aug 2019-March 2020 Pupil Number",
                                                                        Format = ReferenceDataValueFormat.Number,
                                                                        TemplateReferenceId = templateReferenceId++,
                                                                        Value = GetData(spreadsheet, 4, org.SpreadsheetRowNumber, 42)
                                                                    },
                                                                    new ReferenceData
                                                                    {
                                                                        Name = "Aug 2019-March 2020 Rate SEN/AP",
                                                                        Format = ReferenceDataValueFormat.Number,
                                                                        TemplateReferenceId = templateReferenceId++,
                                                                        Value = -1 // Not given
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    },
                                    new FundingLine
                                    {
                                        Name = "Early Years Block",
                                        TemplateLineId = 0,
                                        Type = FundingLineType.Information,
                                        Value = GetDataFromMillions(spreadsheet, 1, org.SpreadsheetRowNumber, 10),
                                        FundingLines = new List<FundingLine>
                                        {
                                            new FundingLine
                                            {
                                                Name = "Initial funding funding for universal entitlement for 3 and 4 year olds",
                                                Value = GetDataFromMillions(spreadsheet, 5, org.SpreadsheetRowNumber, 4),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "HN Block Before Basic Entitlement and Import and Export Adjustments",
                                                        Type = CalculationType.PerPupilFunding,
                                                        Value = GetDataFromMillions(spreadsheet, 5, org.SpreadsheetRowNumber, 4),
                                                        ValueFormat = CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "Total 3 and 4 Year Old for Additional Hours for Working Parents (PTE)",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetData(spreadsheet, 5, org.SpreadsheetRowNumber, 3)
                                                            },
                                                            new ReferenceData
                                                            {
                                                                Name = "Early Years Universal Entitlement for 3 and 4 Year Olds Rate",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataInPence(spreadsheet, 5, org.SpreadsheetRowNumber, 2)
                                                            },
                                                            new ReferenceData
                                                            {
                                                                Name = "PTE Funded Hours",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = 15
                                                            }
                                                        }
                                                    }
                                                }
                                            },
                                            new FundingLine
                                            {
                                                Name = "Initial funding for additional 15 hours entitlement for eligible working parents of 3 and 4 year olds",
                                                Value = GetDataFromMillions(spreadsheet, 5, org.SpreadsheetRowNumber, 6),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "HN Block Before Basic Entitlement and Import and Export Adjustments",
                                                        Type = CalculationType.PerPupilFunding,
                                                        Value = GetDataFromMillions(spreadsheet, 5, org.SpreadsheetRowNumber, 6),
                                                        ValueFormat = CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "Total 3 and 4 Year Old for Additional Hours for Working Parents (PTE)",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetData(spreadsheet, 5, org.SpreadsheetRowNumber, 5)
                                                            },
                                                            new ReferenceData
                                                            {
                                                                Name = "Early Years Universal Entitlement for 3 and 4 Year Olds Rate",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataInPence(spreadsheet, 5, org.SpreadsheetRowNumber, 2)
                                                            },
                                                            new ReferenceData
                                                            {
                                                                Name = "PTE Funded Hours",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = 15
                                                            }
                                                        }
                                                    }
                                                }
                                            },
                                            new FundingLine
                                            {
                                                Name = "Initial funding funding for 2 year old entitlement",
                                                Value = GetDataFromMillions(spreadsheet, 5, org.SpreadsheetRowNumber, 9),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "Per Pupil Funding for 2 year old entitlement",
                                                        Type = CalculationType.LumpSum,
                                                        Value =  GetDataFromMillions(spreadsheet, 5, org.SpreadsheetRowNumber, 9),
                                                        ValueFormat = CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "Total 2 Year Olds (PTE)",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetData(spreadsheet, 5, org.SpreadsheetRowNumber, 8)
                                                            },
                                                            new ReferenceData
                                                            {
                                                                Name = "LA hourly rate for 2 year old entitlement",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataInPence(spreadsheet, 5, org.SpreadsheetRowNumber, 7)
                                                            },
                                                            new ReferenceData
                                                            {
                                                                Name = "PTE Funded Hours",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = 15
                                                            }
                                                        }
                                                    }
                                                }
                                            },
                                            new FundingLine
                                            {
                                                Name = "Early Years Pupil Premium",
                                                Value = GetDataFromMillions(spreadsheet, 5, org.SpreadsheetRowNumber, 10),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "Early Years Pupil Premium lumpsum",
                                                        Type = CalculationType.LumpSum,
                                                        Value = GetDataFromMillions(spreadsheet, 5, org.SpreadsheetRowNumber, 10),
                                                        ValueFormat = CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "Early Years Pupil Premium Rate",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = -1
                                                            }
                                                        }
                                                    }
                                                }
                                            },
                                            new FundingLine
                                            {
                                                Name = "Disability Access Fund",
                                                Value = GetDataFromMillions(spreadsheet, 5, org.SpreadsheetRowNumber, 11),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "Disability Access Fund lumpsum",
                                                        Type = CalculationType.LumpSum,
                                                        Value = GetDataFromMillions(spreadsheet, 5, org.SpreadsheetRowNumber, 11),
                                                        ValueFormat = CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "Disability Access Fund Rate",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = -1
                                                            }
                                                        }
                                                    }
                                                }
                                            },
                                            new FundingLine
                                            {
                                                Name = "Maintained Nursery School Supplementary funding",
                                                Value = GetDataFromMillions(spreadsheet, 5, org.SpreadsheetRowNumber, 12),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "Per Pupil Funding for Maintained Nursery Schools",
                                                        Type = CalculationType.PerPupilFunding,
                                                        Value = GetDataFromMillions(spreadsheet, 5, org.SpreadsheetRowNumber, 12),
                                                        ValueFormat = CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "Maintained Nursery Schools Supplement (PTE)",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = -1
                                                            },
                                                            new ReferenceData
                                                            {
                                                                Name = "Maintained Nursery Schools Supplement Hourly Rate",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = -1
                                                            },
                                                            new ReferenceData
                                                            {
                                                                Name = "PTE Funded Hours",
                                                                Format = ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = -1
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        },
            };
        }

        /// <summary>
        /// Get all the periods that make up DSG.
        /// </summary>
        /// <param name="periodValue">The period value (in pence).</param>
        /// <returns>A list of funding line periods.</returns>
        private static List<FundingLinePeriod> GetPeriods(long periodValue)
        {
            return new List<FundingLinePeriod>
            {
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    DistributionPeriodId = "FY1920",
                    Year = 2019,
                    TypeValue = "April",
                    Occurrence = 1,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    DistributionPeriodId = "FY1920",
                    Year = 2019,
                    TypeValue = "April",
                    Occurrence = 2,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    DistributionPeriodId = "FY1920",
                    Year = 2019,
                    TypeValue = "April",
                    Occurrence = 3,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    DistributionPeriodId = "FY1920",
                    Year = 2019,
                    TypeValue = "May",
                    Occurrence = 1,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    DistributionPeriodId = "FY1920",
                    Year = 2019,
                    TypeValue = "May",
                    Occurrence = 2,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    DistributionPeriodId = "FY1920",
                    Year = 2019,
                    TypeValue = "June",
                    Occurrence = 1,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    DistributionPeriodId = "FY1920",
                    Year = 2019,
                    TypeValue = "June",
                    Occurrence = 2,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    DistributionPeriodId = "FY1920",
                    Year = 2019,
                    TypeValue = "July",
                    Occurrence = 1,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    DistributionPeriodId = "FY1920",
                    Year = 2019,
                    TypeValue = "July",
                    Occurrence = 2,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    DistributionPeriodId = "FY1920",
                    Year = 2019,
                    TypeValue = "August",
                    Occurrence = 1,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    DistributionPeriodId = "FY1920",
                    Year = 2019,
                    TypeValue = "August",
                    Occurrence = 2,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    DistributionPeriodId = "FY1920",
                    Year = 2019,
                    TypeValue = "September",
                    Occurrence = 1,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    DistributionPeriodId = "FY1920",
                    Year = 2019,
                    TypeValue = "September",
                    Occurrence = 2,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    DistributionPeriodId = "FY1920",
                    Year = 2019,
                    TypeValue = "October",
                    Occurrence = 1,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    DistributionPeriodId = "FY1920",
                    Year = 2019,
                    TypeValue = "October",
                    Occurrence = 2,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    DistributionPeriodId = "FY1920",
                    Year = 2019,
                    TypeValue = "November",
                    Occurrence = 1,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    DistributionPeriodId = "FY1920",
                    Year = 2019,
                    TypeValue = "November",
                    Occurrence = 2,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    DistributionPeriodId = "FY1920",
                    Year = 2019,
                    TypeValue = "December",
                    Occurrence = 1,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    DistributionPeriodId = "FY1920",
                    Year = 2019,
                    TypeValue = "December",
                    Occurrence = 2,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    DistributionPeriodId = "FY1920",
                    Year = 2020,
                    TypeValue = "January",
                    Occurrence = 1,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    DistributionPeriodId = "FY1920",
                    Year = 2020,
                    TypeValue = "January",
                    Occurrence = 2,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    DistributionPeriodId = "FY1920",
                    Year = 2020,
                    TypeValue = "Febuary",
                    Occurrence = 1,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    DistributionPeriodId = "FY1920",
                    Year = 2020,
                    TypeValue = "Febuary",
                    Occurrence = 2,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    DistributionPeriodId = "FY1920",
                    Year = 2020,
                    TypeValue = "March",
                    Occurrence = 1,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    DistributionPeriodId = "FY1920",
                    Year = 2020,
                    TypeValue = "March",
                    Occurrence = 2,
                    ProfiledValue = periodValue
                }
            };
        }

        /// <summary>
        /// Get all the LAs for a region.
        /// </summary>
        /// <param name="regionName">The name of a region (as comes from the spreadsheet).</param>
        /// <returns>A list of organisations.</returns>
        private static List<Org> GetLasForRegion(string regionName)
        {
            switch (regionName)
            {
                case "London":
                case "LONDON":
                    return new List<Org>
                    {
                        //new Org { Code = "201", Name = "City of London", Type = Type.LA, UKPRN = "MOCKUKPRN201", RowNumber = 5 },
                        new Org { Code = "202", Name = "Camden", Type = Type.LA, UKPRN = "MOCKUKPRN202", SpreadsheetRowNumber = 6 },
                        new Org { Code = "203", Name = "Greenwich", Type = Type.LA, UKPRN = "MOCKUKPRN203", SpreadsheetRowNumber = 7 },
                        new Org { Code = "204", Name = "Hackney", Type = Type.LA, UKPRN = "MOCKUKPRN204", SpreadsheetRowNumber = 8 },
                        new Org { Code = "205", Name = "Hammersmith and Fulham", Type = Type.LA, UKPRN = "MOCKUKPRN205", SpreadsheetRowNumber = 9 },
                        new Org { Code = "206", Name = "Islington", Type = Type.LA, UKPRN = "MOCKUKPRN206", SpreadsheetRowNumber = 10 },
                        new Org { Code = "207", Name = "Kensington and Chelsea", Type = Type.LA, UKPRN = "MOCKUKPRN207", SpreadsheetRowNumber = 11 },
                        new Org { Code = "208", Name = "Lambeth", Type = Type.LA, UKPRN = "MOCKUKPRN208", SpreadsheetRowNumber = 12 },
                        new Org { Code = "209", Name = "Lewisham", Type = Type.LA, UKPRN = "MOCKUKPRN209", SpreadsheetRowNumber = 13 },
                        new Org { Code = "210", Name = "Southwark", Type = Type.LA, UKPRN = "MOCKUKPRN210", SpreadsheetRowNumber = 14 },
                        new Org { Code = "211", Name = "Tower Hamlets", Type = Type.LA, UKPRN = "MOCKUKPRN211", SpreadsheetRowNumber = 15 },
                        new Org { Code = "212", Name = "Wandsworth", Type = Type.LA, UKPRN = "MOCKUKPRN212", SpreadsheetRowNumber = 16 },
                        new Org { Code = "213", Name = "Westminster", Type = Type.LA, UKPRN = "MOCKUKPRN213", SpreadsheetRowNumber = 17 },
                        new Org { Code = "301", Name = "Barking and Dagenham", Type = Type.LA, UKPRN = "MOCKUKPRN301", SpreadsheetRowNumber = 18 },
                        new Org { Code = "302", Name = "Barnet", Type = Type.LA, UKPRN = "MOCKUKPRN302", SpreadsheetRowNumber = 19 },
                        new Org { Code = "303", Name = "Bexley", Type = Type.LA, UKPRN = "MOCKUKPRN303", SpreadsheetRowNumber = 20 },
                        new Org { Code = "304", Name = "Brent", Type = Type.LA, UKPRN = "MOCKUKPRN304", SpreadsheetRowNumber = 21 },
                        new Org { Code = "305", Name = "Bromley", Type = Type.LA, UKPRN = "MOCKUKPRN305", SpreadsheetRowNumber = 22 },
                        new Org { Code = "306", Name = "Croydon", Type = Type.LA, UKPRN = "MOCKUKPRN306", SpreadsheetRowNumber = 23 },
                        new Org { Code = "307", Name = "Ealing", Type = Type.LA, UKPRN = "MOCKUKPRN307", SpreadsheetRowNumber = 24 },
                        new Org { Code = "308", Name = "Enfield", Type = Type.LA, UKPRN = "MOCKUKPRN308", SpreadsheetRowNumber = 25 },
                        new Org { Code = "309", Name = "Haringey", Type = Type.LA, UKPRN = "MOCKUKPRN309", SpreadsheetRowNumber = 26 },
                        new Org { Code = "310", Name = "Harrow", Type = Type.LA, UKPRN = "MOCKUKPRN310", SpreadsheetRowNumber = 27 },
                        new Org { Code = "311", Name = "Havering", Type = Type.LA, UKPRN = "MOCKUKPRN311", SpreadsheetRowNumber = 28 },
                        new Org { Code = "312", Name = "Hillingdon", Type = Type.LA, UKPRN = "MOCKUKPRN312", SpreadsheetRowNumber = 29 },
                        new Org { Code = "313", Name = "Hounslow", Type = Type.LA, UKPRN = "MOCKUKPRN313", SpreadsheetRowNumber = 30 },
                        new Org { Code = "314", Name = "Kingston upon Thames", Type = Type.LA, UKPRN = "MOCKUKPRN314", SpreadsheetRowNumber = 31 },
                        new Org { Code = "315", Name = "Merton", Type = Type.LA, UKPRN = "MOCKUKPRN315", SpreadsheetRowNumber = 32 },
                        new Org { Code = "316", Name = "Newham", Type = Type.LA, UKPRN = "MOCKUKPRN316", SpreadsheetRowNumber = 33 },
                        new Org { Code = "317", Name = "Redbridge", Type = Type.LA, UKPRN = "MOCKUKPRN317", SpreadsheetRowNumber = 34 },
                        new Org { Code = "318", Name = "Richmond upon Thames", Type = Type.LA, UKPRN = "MOCKUKPRN318", SpreadsheetRowNumber = 35 },
                        new Org { Code = "319", Name = "Sutton", Type = Type.LA, UKPRN = "MOCKUKPRN319", SpreadsheetRowNumber = 36 },
                        new Org { Code = "320", Name = "Waltham Forest", Type = Type.LA, UKPRN = "MOCKUKPRN320", SpreadsheetRowNumber = 37 },
                    };
                case "METROPOLITAN AUTHORITIES":
                    return new List<Org>
                    {
                        new Org { Code = "330", Name = "Birmingham", Type = Type.LA, UKPRN = "MOCKUKPRN330", SpreadsheetRowNumber = 37 },
                        new Org { Code = "331", Name = "Coventry", Type = Type.LA, UKPRN = "MOCKUKPRN331", SpreadsheetRowNumber = 38 },
                        new Org { Code = "332", Name = "Dudley", Type = Type.LA, UKPRN = "MOCKUKPRN332", SpreadsheetRowNumber = 39 },
                        new Org { Code = "333", Name = "Sandwell", Type = Type.LA, UKPRN = "MOCKUKPRN333", SpreadsheetRowNumber = 40 },
                        new Org { Code = "334", Name = "Solihull", Type = Type.LA, UKPRN = "MOCKUKPRN334", SpreadsheetRowNumber = 41 },
                        new Org { Code = "335", Name = "Walsall", Type = Type.LA, UKPRN = "MOCKUKPRN335", SpreadsheetRowNumber = 42 },
                        new Org { Code = "336", Name = "Wolverhampton", Type = Type.LA, UKPRN = "MOCKUKPRN336", SpreadsheetRowNumber = 43 },
                        new Org { Code = "340", Name = "Knowsley", Type = Type.LA, UKPRN = "MOCKUKPRN340", SpreadsheetRowNumber = 44 },
                        new Org { Code = "341", Name = "Liverpool", Type = Type.LA, UKPRN = "MOCKUKPRN341", SpreadsheetRowNumber = 45 },
                        new Org { Code = "342", Name = "St Helens", Type = Type.LA, UKPRN = "MOCKUKPRN342", SpreadsheetRowNumber = 46 },
                        new Org { Code = "343", Name = "Sefton", Type = Type.LA, UKPRN = "MOCKUKPRN343", SpreadsheetRowNumber = 47 },
                        new Org { Code = "344", Name = "Wirral", Type = Type.LA, UKPRN = "MOCKUKPRN344", SpreadsheetRowNumber = 48 },
                        new Org { Code = "350", Name = "Bolton", Type = Type.LA, UKPRN = "MOCKUKPRN350", SpreadsheetRowNumber = 49 },
                        new Org { Code = "351", Name = "Bury", Type = Type.LA, UKPRN = "MOCKUKPRN351", SpreadsheetRowNumber = 50 },
                        new Org { Code = "352", Name = "Manchester", Type = Type.LA, UKPRN = "MOCKUKPRN352", SpreadsheetRowNumber = 51 },
                        new Org { Code = "353", Name = "Oldham", Type = Type.LA, UKPRN = "MOCKUKPRN353", SpreadsheetRowNumber = 52 },
                        new Org { Code = "354", Name = "Rochdale", Type = Type.LA, UKPRN = "MOCKUKPRN354", SpreadsheetRowNumber = 53 },
                        new Org { Code = "355", Name = "Salford", Type = Type.LA, UKPRN = "MOCKUKPRN355", SpreadsheetRowNumber = 54 },
                        new Org { Code = "356", Name = "Stockport", Type = Type.LA, UKPRN = "MOCKUKPRN356", SpreadsheetRowNumber = 55 },
                        new Org { Code = "357", Name = "Tameside", Type = Type.LA, UKPRN = "MOCKUKPRN357", SpreadsheetRowNumber = 56 },
                        new Org { Code = "358", Name = "Trafford", Type = Type.LA, UKPRN = "MOCKUKPRN358", SpreadsheetRowNumber = 57 },
                        new Org { Code = "359", Name = "Wigan", Type = Type.LA, UKPRN = "MOCKUKPRN359", SpreadsheetRowNumber = 58 }
                    };
                case "UNITARY AUTHORITIES":
                    return new List<Org>
                    {
                        new Org { Code = "800", Name = "Bath and North East Somerset", Type = Type.LA, UKPRN = "MOCKUKPRN800", SpreadsheetRowNumber = 74 },
                        new Org { Code = "801", Name = "Bristol, City of", Type = Type.LA, UKPRN = "MOCKUKPRN801", SpreadsheetRowNumber = 75 },
                        new Org { Code = "802", Name = "North Somerset", Type = Type.LA, UKPRN = "MOCKUKPRN802", SpreadsheetRowNumber = 76 },
                        new Org { Code = "803", Name = "South Gloucestershire", Type = Type.LA, UKPRN = "MOCKUKPRN803", SpreadsheetRowNumber = 77 },
                        new Org { Code = "805", Name = "Hartlepool", Type = Type.LA, UKPRN = "MOCKUKPRN805", SpreadsheetRowNumber = 78 },
                        new Org { Code = "806", Name = "Middlesbrough", Type = Type.LA, UKPRN = "MOCKUKPRN806", SpreadsheetRowNumber = 79 },
                        new Org { Code = "807", Name = "Redcar and Cleveland", Type = Type.LA, UKPRN = "MOCKUKPRN807", SpreadsheetRowNumber = 80 },
                        new Org { Code = "808", Name = "Stockton-on-Tees", Type = Type.LA, UKPRN = "MOCKUKPRN808", SpreadsheetRowNumber = 81 },
                        new Org { Code = "810", Name = "Kingston Upon Hull, City of", Type = Type.LA, UKPRN = "MOCKUKPRN810", SpreadsheetRowNumber = 82 },
                        new Org { Code = "811", Name = "East Riding of Yorkshire", Type = Type.LA, UKPRN = "MOCKUKPRN811", SpreadsheetRowNumber = 83 },
                        new Org { Code = "812", Name = "North East Lincolnshire", Type = Type.LA, UKPRN = "MOCKUKPRN812", SpreadsheetRowNumber = 84 },
                        new Org { Code = "813", Name = "North Lincolnshire", Type = Type.LA, UKPRN = "MOCKUKPRN813", SpreadsheetRowNumber = 85 },
                        new Org { Code = "816", Name = "York", Type = Type.LA, UKPRN = "MOCKUKPRN816", SpreadsheetRowNumber = 87 },
                        new Org { Code = "821", Name = "Luton", Type = Type.LA, UKPRN = "MOCKUKPRN821", SpreadsheetRowNumber = 88 },
                        new Org { Code = "822", Name = "Bedford Borough", Type = Type.LA, UKPRN = "MOCKUKPRN822", SpreadsheetRowNumber = 89 },
                        new Org { Code = "823", Name = "Central Bedfordshire", Type = Type.LA, UKPRN = "MOCKUKPRN823", SpreadsheetRowNumber = 90 },
                        new Org { Code = "826", Name = "Milton Keynes", Type = Type.LA, UKPRN = "MOCKUKPRN826", SpreadsheetRowNumber = 92 },
                        new Org { Code = "831", Name = "Derby", Type = Type.LA, UKPRN = "MOCKUKPRN831", SpreadsheetRowNumber = 94 },
                        new Org { Code = "838", Name = "Dorset", Type = Type.LA, UKPRN = "MOCKUKPRN838", SpreadsheetRowNumber = 95 },
                        new Org { Code = "839", Name = "Bournemouth, Poole and Dorset", Type = Type.LA, UKPRN = "MOCKUKPRN839", SpreadsheetRowNumber = 96 },
                        new Org { Code = "840", Name = "Durham", Type = Type.LA, UKPRN = "MOCKUKPRN840", SpreadsheetRowNumber = 97 },
                        new Org { Code = "841", Name = "Darlington", Type = Type.LA, UKPRN = "MOCKUKPRN841", SpreadsheetRowNumber = 98 },
                        new Org { Code = "846", Name = "Brighton and Hove", Type = Type.LA, UKPRN = "MOCKUKPRN846", SpreadsheetRowNumber = 100 },
                        new Org { Code = "851", Name = "Portsmouth", Type = Type.LA, UKPRN = "MOCKUKPRN851", SpreadsheetRowNumber = 102 },
                        new Org { Code = "852", Name = "Southampton", Type = Type.LA, UKPRN = "MOCKUKPRN852", SpreadsheetRowNumber = 103 },
                        new Org { Code = "856", Name = "Leicester", Type = Type.LA, UKPRN = "MOCKUKPRN856", SpreadsheetRowNumber = 105 },
                        new Org { Code = "857", Name = "Rutland", Type = Type.LA, UKPRN = "MOCKUKPRN857", SpreadsheetRowNumber = 106 },
                        new Org { Code = "861", Name = "Stoke-on-Trent", Type = Type.LA, UKPRN = "MOCKUKPRN861", SpreadsheetRowNumber = 108 },
                        new Org { Code = "865", Name = "Wiltshire", Type = Type.LA, UKPRN = "MOCKUKPRN865", SpreadsheetRowNumber = 109 },
                        new Org { Code = "866", Name = "Swindon", Type = Type.LA, UKPRN = "MOCKUKPRN866", SpreadsheetRowNumber = 110 },
                        new Org { Code = "867", Name = "Bracknell Forest", Type = Type.LA, UKPRN = "MOCKUKPRN867", SpreadsheetRowNumber = 111 },
                        new Org { Code = "868", Name = "Windsor and Maidenhead", Type = Type.LA, UKPRN = "MOCKUKPRN868", SpreadsheetRowNumber = 112 },
                        new Org { Code = "869", Name = "West Berkshire", Type = Type.LA, UKPRN = "MOCKUKPRN869", SpreadsheetRowNumber = 113 },
                        new Org { Code = "870", Name = "Reading", Type = Type.LA, UKPRN = "MOCKUKPRN870", SpreadsheetRowNumber = 114 },
                        new Org { Code = "871", Name = "Slough", Type = Type.LA, UKPRN = "MOCKUKPRN871", SpreadsheetRowNumber = 115 },
                        new Org { Code = "872", Name = "Wokingham", Type = Type.LA, UKPRN = "MOCKUKPRN872", SpreadsheetRowNumber = 116 },
                        new Org { Code = "874", Name = "Peterborough", Type = Type.LA, UKPRN = "MOCKUKPRN874", SpreadsheetRowNumber = 118 },
                        new Org { Code = "876", Name = "Halton", Type = Type.LA, UKPRN = "MOCKUKPRN876", SpreadsheetRowNumber = 119 },
                        new Org { Code = "877", Name = "Warrington", Type = Type.LA, UKPRN = "MOCKUKPRN877", SpreadsheetRowNumber = 120 },
                        new Org { Code = "879", Name = "Plymouth", Type = Type.LA, UKPRN = "MOCKUKPRN879", SpreadsheetRowNumber = 122 },
                        new Org { Code = "880", Name = "Torbay", Type = Type.LA, UKPRN = "MOCKUKPRN880", SpreadsheetRowNumber = 123 },
                        new Org { Code = "882", Name = "Southend-on-Sea", Type = Type.LA, UKPRN = "MOCKUKPRN882", SpreadsheetRowNumber = 125 },
                        new Org { Code = "883", Name = "Thurrock", Type = Type.LA, UKPRN = "MOCKUKPRN883", SpreadsheetRowNumber = 126 },
                        new Org { Code = "884", Name = "Herefordshire", Type = Type.LA, UKPRN = "MOCKUKPRN884", SpreadsheetRowNumber = 127 },
                        new Org { Code = "887", Name = "Medway", Type = Type.LA, UKPRN = "MOCKUKPRN887", SpreadsheetRowNumber = 130 },
                        new Org { Code = "889", Name = "Blackburn with Darwen", Type = Type.LA, UKPRN = "MOCKUKPRN889", SpreadsheetRowNumber = 132 },
                        new Org { Code = "890", Name = "Blackpool", Type = Type.LA, UKPRN = "MOCKUKPRN890", SpreadsheetRowNumber = 133 },
                        new Org { Code = "892", Name = "Nottingham", Type = Type.LA, UKPRN = "MOCKUKPRN892", SpreadsheetRowNumber = 135 },
                        new Org { Code = "893", Name = "Shropshire", Type = Type.LA, UKPRN = "MOCKUKPRN893", SpreadsheetRowNumber = 136 },
                        new Org { Code = "894", Name = "Telford and Wrekin", Type = Type.LA, UKPRN = "MOCKUKPRN894", SpreadsheetRowNumber = 137 },
                        new Org { Code = "895", Name = "Cheshire East", Type = Type.LA, UKPRN = "MOCKUKPRN895", SpreadsheetRowNumber = 138 },
                        new Org { Code = "896", Name = "Cheshire West and Chester", Type = Type.LA, UKPRN = "MOCKUKPRN896", SpreadsheetRowNumber = 139 },
                        new Org { Code = "908", Name = "Cornwall", Type = Type.LA, UKPRN = "MOCKUKPRN908", SpreadsheetRowNumber = 140 },
                        new Org { Code = "919", Name = "Hertfordshire", Type = Type.LA, UKPRN = "MOCKUKPRN919", SpreadsheetRowNumber = 143 },
                        new Org { Code = "921", Name = "Isle of Wight", Type = Type.LA, UKPRN = "MOCKUKPRN921", SpreadsheetRowNumber = 144 },
                        new Org { Code = "929", Name = "Northumberland", Type = Type.LA, UKPRN = "MOCKUKPRN929", SpreadsheetRowNumber = 148 }
                    };
                case "UPPER TIER AUTHORITIES":
                    return new List<Org>
                    {
                        new Org { Code = "815", Name = "North Yorkshire", Type = Type.LA, UKPRN = "MOCKUKPRN815", SpreadsheetRowNumber = 86 },
                        new Org { Code = "825", Name = "Buckinghamshire", Type = Type.LA, UKPRN = "MOCKUKPRN825", SpreadsheetRowNumber = 91 },
                        new Org { Code = "830", Name = "Derbyshire", Type = Type.LA, UKPRN = "MOCKUKPRN830", SpreadsheetRowNumber = 93 },
                        new Org { Code = "845", Name = "East Sussex", Type = Type.LA, UKPRN = "MOCKUKPRN845", SpreadsheetRowNumber = 99 },
                        new Org { Code = "850", Name = "Hampshire", Type = Type.LA, UKPRN = "MOCKUKPRN850", SpreadsheetRowNumber = 101 },
                        new Org { Code = "860", Name = "Staffordshire", Type = Type.LA, UKPRN = "MOCKUKPRN860", SpreadsheetRowNumber = 107 },
                        new Org { Code = "873", Name = "Cambridgeshire", Type = Type.LA, UKPRN = "MOCKUKPRN873", SpreadsheetRowNumber = 117 },
                        new Org { Code = "878", Name = "Devon", Type = Type.LA, UKPRN = "MOCKUKPRN878", SpreadsheetRowNumber = 121 },
                        new Org { Code = "881", Name = "Essex", Type = Type.LA, UKPRN = "MOCKUKPRN881", SpreadsheetRowNumber = 124 },
                        new Org { Code = "885", Name = "Worcestershire", Type = Type.LA, UKPRN = "MOCKUKPRN885", SpreadsheetRowNumber = 128 },
                        new Org { Code = "888", Name = "Lancashire", Type = Type.LA, UKPRN = "MOCKUKPRN888", SpreadsheetRowNumber = 131 },
                        new Org { Code = "891", Name = "Nottinghamshire", Type = Type.LA, UKPRN = "MOCKUKPRN891", SpreadsheetRowNumber = 134 },
                        new Org { Code = "909", Name = "Cumbria", Type = Type.LA, UKPRN = "MOCKUKPRN909", SpreadsheetRowNumber = 141 },
                        new Org { Code = "916", Name = "Gloucestershire", Type = Type.LA, UKPRN = "MOCKUKPRN916", SpreadsheetRowNumber = 142 },
                        new Org { Code = "919", Name = "Hertfordshire", Type = Type.LA, UKPRN = "MOCKUKPRN919", SpreadsheetRowNumber = 143 },
                        new Org { Code = "925", Name = "Lincolnshire", Type = Type.LA, UKPRN = "MOCKUKPRN925", SpreadsheetRowNumber = 145 },
                        new Org { Code = "926", Name = "Norfolk", Type = Type.LA, UKPRN = "MOCKUKPRN926", SpreadsheetRowNumber = 146 },
                        new Org { Code = "928", Name = "Northamptonshire", Type = Type.LA, UKPRN = "MOCKUKPRN928", SpreadsheetRowNumber = 147 },
                        new Org { Code = "931", Name = "Oxfordshire", Type = Type.LA, UKPRN = "MOCKUKPRN931", SpreadsheetRowNumber = 149 },
                        new Org { Code = "933", Name = "Somerset", Type = Type.LA, UKPRN = "MOCKUKPRN933", SpreadsheetRowNumber = 150 },
                        new Org { Code = "935", Name = "Suffolk", Type = Type.LA, UKPRN = "MOCKUKPRN935", SpreadsheetRowNumber = 151 },
                        new Org { Code = "936", Name = "Surrey", Type = Type.LA, UKPRN = "MOCKUKPRN936", SpreadsheetRowNumber = 152 },
                        new Org { Code = "937", Name = "Warwickshire", Type = Type.LA, UKPRN = "MOCKUKPRN937", SpreadsheetRowNumber = 153 },
                        new Org { Code = "938", Name = "West Sussex", Type = Type.LA, UKPRN = "MOCKUKPRN938", SpreadsheetRowNumber = 154 }
                    };
                case "East of England":
                    return new List<Org>
                    {
                        new Org { Code = "821", Name = "Luton", Type = Type.LA, UKPRN = "MOCKUKPRN821", SpreadsheetRowNumber = 88 },
                        new Org { Code = "822", Name = "Bedford Borough", Type = Type.LA, UKPRN = "MOCKUKPRN822", SpreadsheetRowNumber = 89 },
                        new Org { Code = "823", Name = "Central Bedfordshire", Type = Type.LA, UKPRN = "MOCKUKPRN823", SpreadsheetRowNumber = 90 },
                        new Org { Code = "873", Name = "Cambridgeshire", Type = Type.LA, UKPRN = "MOCKUKPRN873", SpreadsheetRowNumber = 117 },
                        new Org { Code = "874", Name = "Peterborough", Type = Type.LA, UKPRN = "MOCKUKPRN874", SpreadsheetRowNumber = 118 },
                        new Org { Code = "881", Name = "Essex", Type = Type.LA, UKPRN = "MOCKUKPRN881", SpreadsheetRowNumber = 124 },
                        new Org { Code = "882", Name = "Southend-on-Sea", Type = Type.LA, UKPRN = "MOCKUKPRN882", SpreadsheetRowNumber = 125 },
                        new Org { Code = "883", Name = "Thurrock", Type = Type.LA, UKPRN = "MOCKUKPRN883", SpreadsheetRowNumber = 126 },
                        new Org { Code = "919", Name = "Hertfordshire", Type = Type.LA, UKPRN = "MOCKUKPRN919", SpreadsheetRowNumber = 143 },
                        new Org { Code = "926", Name = "Norfolk", Type = Type.LA, UKPRN = "MOCKUKPRN926", SpreadsheetRowNumber = 146 },
                        new Org { Code = "935", Name = "Suffolk", Type = Type.LA, UKPRN = "MOCKUKPRN935", SpreadsheetRowNumber = 151 }
                    };
                case "East Midlands":
                    return new List<Org>
                    {
                        new Org { Code = "830", Name = "Derbyshire", Type = Type.LA, UKPRN = "MOCKUKPRN830", SpreadsheetRowNumber = 93 },
                        new Org { Code = "831", Name = "Derby", Type = Type.LA, UKPRN = "MOCKUKPRN831", SpreadsheetRowNumber = 94 },
                        new Org { Code = "855", Name = "Leicestershire", Type = Type.LA, UKPRN = "MOCKUKPRN855", SpreadsheetRowNumber = 104 },
                        new Org { Code = "856", Name = "Leicester", Type = Type.LA, UKPRN = "MOCKUKPRN856", SpreadsheetRowNumber = 105 },
                        new Org { Code = "857", Name = "Rutland", Type = Type.LA, UKPRN = "MOCKUKPRN857", SpreadsheetRowNumber = 106 },
                        new Org { Code = "891", Name = "Nottinghamshire", Type = Type.LA, UKPRN = "MOCKUKPRN891", SpreadsheetRowNumber = 134 },
                        new Org { Code = "892", Name = "Nottingham", Type = Type.LA, UKPRN = "MOCKUKPRN892", SpreadsheetRowNumber = 135 },
                        new Org { Code = "925", Name = "Lincolnshire", Type = Type.LA, UKPRN = "MOCKUKPRN925", SpreadsheetRowNumber = 145 },
                        new Org { Code = "928", Name = "Northamptonshire", Type = Type.LA, UKPRN = "MOCKUKPRN928", SpreadsheetRowNumber = 147 }
                    };
                case "North East":
                    return new List<Org>
                    {
                        new Org { Code = "390", Name = "Gateshead", Type = Type.LA, UKPRN = "MOCKUKPRN390", SpreadsheetRowNumber = 69 },
                        new Org { Code = "391", Name = "Newcastle upon Tyne", Type = Type.LA, UKPRN = "MOCKUKPRN391", SpreadsheetRowNumber = 70 },
                        new Org { Code = "392", Name = "North Tyneside", Type = Type.LA, UKPRN = "MOCKUKPRN392", SpreadsheetRowNumber = 71 },
                        new Org { Code = "393", Name = "South Tyneside", Type = Type.LA, UKPRN = "MOCKUKPRN393", SpreadsheetRowNumber = 72 },
                        new Org { Code = "394", Name = "Sunderland", Type = Type.LA, UKPRN = "MOCKUKPRN394", SpreadsheetRowNumber = 73 },
                        new Org { Code = "805", Name = "Hartlepool", Type = Type.LA, UKPRN = "MOCKUKPRN805", SpreadsheetRowNumber = 78 },
                        new Org { Code = "806", Name = "Middlesbrough", Type = Type.LA, UKPRN = "MOCKUKPRN806", SpreadsheetRowNumber = 79 },
                        new Org { Code = "807", Name = "Redcar and Cleveland", Type = Type.LA, UKPRN = "MOCKUKPRN807", SpreadsheetRowNumber = 80 },
                        new Org { Code = "808", Name = "Stockton-on-Tees", Type = Type.LA, UKPRN = "MOCKUKPRN808", SpreadsheetRowNumber = 81 },
                        new Org { Code = "840", Name = "Durham", Type = Type.LA, UKPRN = "MOCKUKPRN840", SpreadsheetRowNumber = 97 },
                        new Org { Code = "841", Name = "Darlington", Type = Type.LA, UKPRN = "MOCKUKPRN841", SpreadsheetRowNumber = 98 },
                        new Org { Code = "929", Name = "Northumberland", Type = Type.LA, UKPRN = "MOCKUKPRN929", SpreadsheetRowNumber = 148 }
                    };
                case "North West":
                    return new List<Org>
                    {
                        new Org { Code = "340", Name = "Knowsley", Type = Type.LA, UKPRN = "MOCKUKPRN340", SpreadsheetRowNumber = 45 },
                        new Org { Code = "341", Name = "Liverpool", Type = Type.LA, UKPRN = "MOCKUKPRN341", SpreadsheetRowNumber = 46 },
                        new Org { Code = "342", Name = "St Helens", Type = Type.LA, UKPRN = "MOCKUKPRN342", SpreadsheetRowNumber = 47 },
                        new Org { Code = "343", Name = "Sefton", Type = Type.LA, UKPRN = "MOCKUKPRN343", SpreadsheetRowNumber = 48 },
                        new Org { Code = "344", Name = "Wirral", Type = Type.LA, UKPRN = "MOCKUKPRN344", SpreadsheetRowNumber = 49 },
                        new Org { Code = "350", Name = "Bolton", Type = Type.LA, UKPRN = "MOCKUKPRN350", SpreadsheetRowNumber = 50 },
                        new Org { Code = "351", Name = "Bury", Type = Type.LA, UKPRN = "MOCKUKPRN351", SpreadsheetRowNumber = 51 },
                        new Org { Code = "352", Name = "Manchester", Type = Type.LA, UKPRN = "MOCKUKPRN352", SpreadsheetRowNumber = 52 },
                        new Org { Code = "353", Name = "Oldham", Type = Type.LA, UKPRN = "MOCKUKPRN353", SpreadsheetRowNumber = 53 },
                        new Org { Code = "354", Name = "Rochdale", Type = Type.LA, UKPRN = "MOCKUKPRN354", SpreadsheetRowNumber = 54 },
                        new Org { Code = "355", Name = "Salford", Type = Type.LA, UKPRN = "MOCKUKPRN355", SpreadsheetRowNumber = 55 },
                        new Org { Code = "356", Name = "Stockport", Type = Type.LA, UKPRN = "MOCKUKPRN356", SpreadsheetRowNumber = 56 },
                        new Org { Code = "357", Name = "Tameside", Type = Type.LA, UKPRN = "MOCKUKPRN357", SpreadsheetRowNumber = 57 },
                        new Org { Code = "358", Name = "Trafford", Type = Type.LA, UKPRN = "MOCKUKPRN358", SpreadsheetRowNumber = 58 },
                        new Org { Code = "359", Name = "Wigan", Type = Type.LA, UKPRN = "MOCKUKPRN359", SpreadsheetRowNumber = 59 },
                        new Org { Code = "876", Name = "Halton", Type = Type.LA, UKPRN = "MOCKUKPRN876", SpreadsheetRowNumber = 119 },
                        new Org { Code = "877", Name = "Warrington", Type = Type.LA, UKPRN = "MOCKUKPRN877", SpreadsheetRowNumber = 120 },
                        new Org { Code = "895", Name = "Cheshire East", Type = Type.LA, UKPRN = "MOCKUKPRN895", SpreadsheetRowNumber = 138 },
                        new Org { Code = "896", Name = "Cheshire West and Chester", Type = Type.LA, UKPRN = "MOCKUKPRN896", SpreadsheetRowNumber = 139 },
                        new Org { Code = "909", Name = "Cumbria", Type = Type.LA, UKPRN = "MOCKUKPRN909", SpreadsheetRowNumber = 141 }
                    };
                case "South East":
                    return new List<Org>
                    {
                        new Org { Code = "825", Name = "Buckinghamshire", Type = Type.LA, UKPRN = "MOCKUKPRN825", SpreadsheetRowNumber = 91 },
                        new Org { Code = "826", Name = "Milton Keynes", Type = Type.LA, UKPRN = "MOCKUKPRN826", SpreadsheetRowNumber = 92 },
                        new Org { Code = "845", Name = "East Sussex", Type = Type.LA, UKPRN = "MOCKUKPRN845", SpreadsheetRowNumber = 99 },
                        new Org { Code = "846", Name = "Brighton and Hove", Type = Type.LA, UKPRN = "MOCKUKPRN846", SpreadsheetRowNumber = 100 },
                        new Org { Code = "850", Name = "Hampshire", Type = Type.LA, UKPRN = "MOCKUKPRN850", SpreadsheetRowNumber = 101 },
                        new Org { Code = "851", Name = "Portsmouth", Type = Type.LA, UKPRN = "MOCKUKPRN851", SpreadsheetRowNumber = 102 },
                        new Org { Code = "852", Name = "Southampton", Type = Type.LA, UKPRN = "MOCKUKPRN852", SpreadsheetRowNumber = 103 },
                        new Org { Code = "867", Name = "Bracknell Forest", Type = Type.LA, UKPRN = "MOCKUKPRN867", SpreadsheetRowNumber = 111 },
                        new Org { Code = "868", Name = "Windsor and Maidenhead", Type = Type.LA, UKPRN = "MOCKUKPRN868", SpreadsheetRowNumber = 112 },
                        new Org { Code = "869", Name = "West Berkshire", Type = Type.LA, UKPRN = "MOCKUKPRN869", SpreadsheetRowNumber = 113 },
                        new Org { Code = "870", Name = "Reading", Type = Type.LA, UKPRN = "MOCKUKPRN870", SpreadsheetRowNumber = 114 },
                        new Org { Code = "871", Name = "Slough", Type = Type.LA, UKPRN = "MOCKUKPRN871", SpreadsheetRowNumber = 115 },
                        new Org { Code = "886", Name = "Kent", Type = Type.LA, UKPRN = "MOCKUKPRN886", SpreadsheetRowNumber = 129 },
                        new Org { Code = "887", Name = "Medway", Type = Type.LA, UKPRN = "MOCKUKPRN887", SpreadsheetRowNumber = 130 },
                        new Org { Code = "921", Name = "Isle of Wight", Type = Type.LA, UKPRN = "MOCKUKPRN921", SpreadsheetRowNumber = 144 },
                        new Org { Code = "931", Name = "Oxfordshire", Type = Type.LA, UKPRN = "MOCKUKPRN931", SpreadsheetRowNumber = 149 },
                        new Org { Code = "936", Name = "Surrey", Type = Type.LA, UKPRN = "MOCKUKPRN936", SpreadsheetRowNumber = 152 },
                        new Org { Code = "938", Name = "West Sussex", Type = Type.LA, UKPRN = "MOCKUKPRN938", SpreadsheetRowNumber = 154 }
                    };
                case "South West":
                    return new List<Org>
                    {
                        new Org { Code = "800", Name = "Bath and North East Somerset", Type = Type.LA, UKPRN = "MOCKUKPRN800", SpreadsheetRowNumber = 74 },
                        new Org { Code = "801", Name = "Bristol, City of", Type = Type.LA, UKPRN = "MOCKUKPRN801", SpreadsheetRowNumber = 75 },
                        new Org { Code = "802", Name = "North Somerset", Type = Type.LA, UKPRN = "MOCKUKPRN802", SpreadsheetRowNumber = 76 },
                        new Org { Code = "803", Name = "South Gloucestershire", Type = Type.LA, UKPRN = "MOCKUKPRN803", SpreadsheetRowNumber = 77 },
                        new Org { Code = "838", Name = "Dorset", Type = Type.LA, UKPRN = "MOCKUKPRN838", SpreadsheetRowNumber = 95 },
                        new Org { Code = "839", Name = "Bournemouth, Poole and Dorset", Type = Type.LA, UKPRN = "MOCKUKPRN839", SpreadsheetRowNumber = 96 },
                        new Org { Code = "866", Name = "Swindon", Type = Type.LA, UKPRN = "MOCKUKPRN866", SpreadsheetRowNumber = 110 },
                        new Org { Code = "867", Name = "Bracknell Forest", Type = Type.LA, UKPRN = "MOCKUKPRN867", SpreadsheetRowNumber = 111 },
                        new Org { Code = "878", Name = "Devon", Type = Type.LA, UKPRN = "MOCKUKPRN878", SpreadsheetRowNumber = 121 },
                        new Org { Code = "879", Name = "Plymouth", Type = Type.LA, UKPRN = "MOCKUKPRN879", SpreadsheetRowNumber = 122 },
                        new Org { Code = "880", Name = "Torbay", Type = Type.LA, UKPRN = "MOCKUKPRN880", SpreadsheetRowNumber = 123 },
                        new Org { Code = "908", Name = "Cornwall", Type = Type.LA, UKPRN = "MOCKUKPRN908", SpreadsheetRowNumber = 140 },
                        new Org { Code = "916", Name = "Gloucestershire", Type = Type.LA, UKPRN = "MOCKUKPRN916", SpreadsheetRowNumber = 142 }
                    };
                case "West Midlands":
                    return new List<Org>
                    {
                        new Org { Code = "330", Name = "Birmingham", Type = Type.LA, UKPRN = "MOCKUKPRN330", SpreadsheetRowNumber = 38 },
                        new Org { Code = "331", Name = "Coventry", Type = Type.LA, UKPRN = "MOCKUKPRN331", SpreadsheetRowNumber = 39 },
                        new Org { Code = "332", Name = "Dudley", Type = Type.LA, UKPRN = "MOCKUKPRN332", SpreadsheetRowNumber = 40 },
                        new Org { Code = "333", Name = "Sandwell", Type = Type.LA, UKPRN = "MOCKUKPRN333", SpreadsheetRowNumber = 41 },
                        new Org { Code = "334", Name = "Solihull", Type = Type.LA, UKPRN = "MOCKUKPRN334", SpreadsheetRowNumber = 42 },
                        new Org { Code = "335", Name = "Walsall", Type = Type.LA, UKPRN = "MOCKUKPRN334", SpreadsheetRowNumber = 43 },
                        new Org { Code = "336", Name = "Wolverhampton", Type = Type.LA, UKPRN = "MOCKUKPRN335", SpreadsheetRowNumber = 44 },
                        new Org { Code = "860", Name = "Staffordshire", Type = Type.LA, UKPRN = "MOCKUKPRN860", SpreadsheetRowNumber = 107 },
                        new Org { Code = "861", Name = "Stoke-on-Trent", Type = Type.LA, UKPRN = "MOCKUKPRN861", SpreadsheetRowNumber = 108 },
                        new Org { Code = "884", Name = "Herefordshire", Type = Type.LA, UKPRN = "MOCKUKPRN884", SpreadsheetRowNumber = 127 },
                        new Org { Code = "885", Name = "Worcestershire", Type = Type.LA, UKPRN = "MOCKUKPRN885", SpreadsheetRowNumber = 128 },
                        new Org { Code = "893", Name = "Shropshire", Type = Type.LA, UKPRN = "MOCKUKPRN893", SpreadsheetRowNumber = 136 },
                        new Org { Code = "894", Name = "Telford and Wrekin", Type = Type.LA, UKPRN = "MOCKUKPRN894", SpreadsheetRowNumber = 137 },
                        new Org { Code = "937", Name = "Warwickshire", Type = Type.LA, UKPRN = "MOCKUKPRN937", SpreadsheetRowNumber = 153 }
                    };
                case "Yorkshire and the Humber":
                    return new List<Org>
                    {
                        new Org { Code = "370", Name = "Barnsley", Type = Type.LA, UKPRN = "MOCKUKPRN370", SpreadsheetRowNumber = 60 },
                        new Org { Code = "371", Name = "Doncaster", Type = Type.LA, UKPRN = "MOCKUKPRN371", SpreadsheetRowNumber = 61 },
                        new Org { Code = "372", Name = "Rotherham", Type = Type.LA, UKPRN = "MOCKUKPRN372", SpreadsheetRowNumber = 62 },
                        new Org { Code = "373", Name = "Sheffield", Type = Type.LA, UKPRN = "MOCKUKPRN373", SpreadsheetRowNumber = 63 },
                        new Org { Code = "380", Name = "Bradford", Type = Type.LA, UKPRN = "MOCKUKPRN380", SpreadsheetRowNumber = 64 },
                        new Org { Code = "381", Name = "Calderdale", Type = Type.LA, UKPRN = "MOCKUKPRN381", SpreadsheetRowNumber = 65 },
                        new Org { Code = "382", Name = "Kirklees", Type = Type.LA, UKPRN = "MOCKUKPRN382", SpreadsheetRowNumber = 66 },
                        new Org { Code = "383", Name = "Leeds", Type = Type.LA, UKPRN = "MOCKUKPRN383", SpreadsheetRowNumber = 67 },
                        new Org { Code = "384", Name = "Wakefield", Type = Type.LA, UKPRN = "MOCKUKPRN384", SpreadsheetRowNumber = 68 },
                        new Org { Code = "810", Name = "Kingston Upon Hull, City of", Type = Type.LA, UKPRN = "MOCKUKPRN810", SpreadsheetRowNumber = 82 },
                        new Org { Code = "811", Name = "East Riding of Yorkshire", Type = Type.LA, UKPRN = "MOCKUKPRN811", SpreadsheetRowNumber = 83 },
                        new Org { Code = "812", Name = "North East Lincolnshire", Type = Type.LA, UKPRN = "MOCKUKPRN812", SpreadsheetRowNumber = 84 },
                        new Org { Code = "813", Name = "North Lincolnshire", Type = Type.LA, UKPRN = "MOCKUKPRN813", SpreadsheetRowNumber = 85 },
                        new Org { Code = "815", Name = "North Yorkshire", Type = Type.LA, UKPRN = "MOCKUKPRN815", SpreadsheetRowNumber = 86 },
                        new Org { Code = "816", Name = "York", Type = Type.LA, UKPRN = "MOCKUKPRN816", SpreadsheetRowNumber = 87 }
                    };
            }

            throw new Exception("Can't find child LAs");
        }

        /// <summary>
        /// A simple representation of an organisation.
        /// </summary>
        private class Org
        {
            /// <summary>
            /// The types of grouping in this spreadsheet.
            /// </summary>
            public Type Type { get; set; }

            /// <summary>
            /// The name of the organisation.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// The organisation identifier.
            /// </summary>
            public string Code { get; set; }

            /// <summary>
            /// The organisations UKPRN (optional).
            /// </summary>
            public string UKPRN { get; set; }

            /// <summary>
            /// The row number.
            /// </summary>
            public int SpreadsheetRowNumber { get; set; }
        }

        /// <summary>
        /// The types of grouping in this spreadsheet.
        /// </summary>
        private enum Type
        {
            LA,
            Region,
            LocalGovernmentGroup
        }
    }
}
