using ExcelDataReader;
using FundingMock.Web.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Linq;

namespace FundingMock.Web.Tools
{
    public static class GenerateDSGFunding
    {
        public static FeedBaseModel[] GenerateFeed(string groupingType, string filterBy, int maxResults, int skip)
        {
            var spreadsheet = GetSpreadsheet();

            var allLas = GetLAs(spreadsheet);
            var las = groupingType == "LA" || string.IsNullOrEmpty(groupingType) ? 
                allLas.Where(item => string.IsNullOrEmpty(filterBy) 
                    || item.Code == filterBy
                    || item.Name.Equals(filterBy, StringComparison.InvariantCultureIgnoreCase)).ToList() : new List<Org>();

            var allRegions = GetRegions(spreadsheet);
            var regions = groupingType == "Region" || string.IsNullOrEmpty(groupingType) ?
                allRegions.Where(item => string.IsNullOrEmpty(filterBy) 
                    || item.Name.Equals(filterBy, StringComparison.InvariantCultureIgnoreCase)).ToList() : new List<Org>();

            var organisations = las.ToList(); // Shallow copy
            organisations.AddRange(regions);

            if (string.IsNullOrEmpty(groupingType) || groupingType == "LocalGovernmentGroup")
            {
                organisations.Add(new Org { Name = "METROPOLITAN AUTHORITIES", RowNumber = 156, Type = Type.LocalGovernmentGroup });
                organisations.Add(new Org { Name = "UNITARY AUTHORITIES", RowNumber = 157, Type = Type.LocalGovernmentGroup });
                organisations.Add(new Org { Name = "UPPER TIER AUTHORITIES", RowNumber = 158, Type = Type.LocalGovernmentGroup });
            }

            var ukOffset = new TimeSpan(0, 0, 0);

            var period = new FundingPeriod
            {
                Code = "FY1920",
                Name = "Financial year 2019-20",
                Type = Enums.PeriodType.FinancialYear,
                StartDate = new DateTimeOffset(2019, 4, 1, 0, 0, 0, ukOffset),
                EndDate = new DateTimeOffset(2020, 3, 31, 0, 0, 0, ukOffset)
            };

            var templateVersion = "1.0";

            var stream = new StreamWithTemplateVersion
            {
                Code = "DSG",
                Name = "Dedicated Schools Grant",
                TemplateVersion = templateVersion,
            };

            var fundingVersion = "1.0";
            var schemaUri = "http://example.org/#schema";
            var schemaVersion = "1.0";

            var returnList = new List<FeedBaseModel>();

            foreach (var organisation in organisations)
            {
                var identifiers = new List<OrganisationIdentifier>();

                if (organisation.Type == Type.LA)
                {
                    identifiers.Add(
                        new OrganisationIdentifier
                        {
                            Type = Enums.OrganisationIdentifierType.LACode,
                            Value = organisation.Code
                        }
                    );

                    identifiers.Add(
                        new OrganisationIdentifier
                        {
                            Type = Enums.OrganisationIdentifierType.UKPRN,
                            Value = organisation.UKPRN
                        }
                    );
                }

                var groupingOrg = new OrganisationGroup
                {
                    Name = organisation.Name,
                    Type = organisation.Type == Type.LA ? Enums.OrganisationType.LocalAuthority 
                        : (organisation.Type  == Type.LocalGovernmentGroup ? Enums.OrganisationType.LocalGovernmentGroup : Enums.OrganisationType.Region),
                    SearchableName = organisation.Name,
                    Identifiers = identifiers
                };
               
                var periodValue = Convert.ToInt64(GetDataFromMillions(spreadsheet, 1, organisation.RowNumber, 11) / 25.0);
                var periods = GetPeriods(periodValue);

                var fundingValue = GetFundingValue(spreadsheet, organisation, periods);

                var primaryId = organisation.Type == Type.Region || organisation.Type == Type.LocalGovernmentGroup ? organisation.Name :
                    groupingOrg.Identifiers.FirstOrDefault(id => id.Type == Enums.OrganisationIdentifierType.UKPRN)?.Value;

                var providerFundings = new List<string>();

                switch (organisation.Type)
                {
                    case Type.LA:
                        providerFundings = new List<string>
                        {
                            $"{stream.Code}_{period.Code}_{primaryId}_{fundingVersion}"
                        };

                        break;
                    case Type.Region:
                    case Type.LocalGovernmentGroup:
                        providerFundings.AddRange(GetLasForRegion(organisation.Name).Select(la => $"{stream.Code}_{period.Code}_{la.UKPRN}_{fundingVersion}").ToList());

                        //foreach (var la in allLas)
                        //{
                        //    providerFundings.Add($"{stream.Code}_{period.Code}_{la.UKPRN}_{fundingVersion}");
                        //}

                        break;
                }

                var data = new FeedBaseModel
                {
                    SchemaUri = schemaUri,
                    SchemaVersion = schemaVersion,
                    Funding = new FundingFeed
                    {
                        Id = $"{stream.Code}_{period.Code}_{groupingOrg.Type}_{primaryId}_{fundingVersion}",
                        FundingStream = stream,
                        FundingPeriod = period,
                        Status = FundingStatus.Released,
                        GroupingReason = organisation.Type == Type.LA ? GroupingReason.Payment : GroupingReason.Information,
                        FundingVersion = fundingVersion,
                        OrganisationGroup = groupingOrg,
                        FundingValue = fundingValue,
                        ProviderFundings = providerFundings,
                        StatusChangedDate = new DateTimeOffset(new DateTime(2019, 3, 1)),
                        ExternalPublicationDate = new DateTimeOffset(new DateTime(2019, 3, 7)),
                        PaymentDate = new DateTimeOffset(new DateTime(2019, 3, 14))
                    },
                };

                returnList.Add(data);
            }

            return returnList.Skip(skip).Take(maxResults).ToArray();
        }

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

            var identifiers = new List<OrganisationIdentifier>();

            if (organisation.Type == Type.LA)
            {
                identifiers.Add(
                    new OrganisationIdentifier
                    {
                        Type = Enums.OrganisationIdentifierType.LACode,
                        Value = organisation.Code
                    }
                );

                identifiers.Add(
                    new OrganisationIdentifier
                    {
                        Type = Enums.OrganisationIdentifierType.UKPRN,
                        Value = organisation.UKPRN
                    }
                );
            }

            var ukOffset = new TimeSpan(0, 0, 0);

            var period = new FundingPeriod
            {
                Code = "FY1920",
                Name = "Financial year 2019-20",
                Type = Enums.PeriodType.FinancialYear,
                StartDate = new DateTimeOffset(2019, 4, 1, 0, 0, 0, ukOffset),
                EndDate = new DateTimeOffset(2020, 3, 31, 0, 0, 0, ukOffset)
            };

            var templateVersion = "1.0";

            var stream = new StreamWithTemplateVersion
            {
                Code = "DSG",
                Name = "Dedicated Schools Grant",
                TemplateVersion = templateVersion,
            };

            var periodValue = Convert.ToInt64(GetDataFromMillions(spreadsheet, 1, organisation.RowNumber, 11) / 25.0);
            var periods = GetPeriods(periodValue);

            var fundingValue = GetFundingValue(spreadsheet, organisation, periods);

            return new ProviderFunding
            {
                Id = id,
                Organisation = new Organisation
                {
                    Name = organisation.Name,
                    SearchableName = organisation.Name,
                    ProviderType = organisation.Type.ToString(),
                    ProviderVersionId = "TBC",
                    Identifiers = identifiers
                },
                FundingPeriodCode = period.Code,
                FundingStreamCode = stream.Code,
                FundingValue = fundingValue
            };
        }

        private static DataSet GetSpreadsheet()
        {
            var filePath = @"C:\Users\foxdi\Downloads\DSG_2019-20_Mar_Tables_Values.xls";

            DataSet spreadsheet;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            using (var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(fileStream))
                {
                    spreadsheet = reader.AsDataSet();
                }
            }

            return spreadsheet;
        }

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
                    RowNumber = idx,
                    Type = Type.LA
                });
            }

            return returnList;
        }

        private static List<Org> GetRegions(DataSet spreadsheet)
        {
            var returnList = new List<Org>();

            for (var idx = 160; idx < 169; idx++)
            {
                returnList.Add(new Org
                {
                    Name = GetDataString(spreadsheet, 1, idx, 0),
                    RowNumber = idx,
                    Type = Type.Region
                });
            }

            return returnList;
        }

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

        private static FundingValue GetFundingValue(DataSet spreadsheet, Org org, List<FundingLinePeriod> periods)
        {
            uint templateLineId = 1;
            uint templateCalculationId = 1;
            uint templateReferenceId = 1;

            return new FundingValue
            {
                TotalValue = GetDataFromMillions(spreadsheet, 1, org.RowNumber, 11),
                FundingValueByDistributionPeriod = new List<FundingValueByDistributionPeriod>
                {
                    new FundingValueByDistributionPeriod
                    {
                        DistributionPeriodCode = "FY1920",
                        Value = GetDataFromMillions(spreadsheet, 1, org.RowNumber, 11),
                        FundingLines = new List<FundingLine>
                        {
                            new FundingLine
                            {
                                Name = "PriorToRecoupment",
                                TemplateLineId = templateLineId++,
                                Type = FundingLineType.Information,
                                Value = GetDataFromMillions(spreadsheet, 1, org.RowNumber, 7),
                                FundingLines = new List<FundingLine>
                                {
                                    new FundingLine
                                    {
                                        Name = "School Block",
                                        TemplateLineId = templateLineId++,
                                        Type = FundingLineType.Information,
                                        Value = GetDataFromMillions(spreadsheet, 1, org.RowNumber, 3),
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
                                                        Type = Enums.CalculationType.LumpSum,
                                                        Value = GetDataFromMillions(spreadsheet, 2, org.RowNumber, 7),
                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "2019-20 funding through the premises and mobility factors",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataFromMillions(spreadsheet, 2, org.RowNumber, 7),
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
                                                        Type = Enums.CalculationType.LumpSum,
                                                        Value = GetDataFromMillions(spreadsheet, 2, org.RowNumber, 8),
                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "2019-20 schools block primary unit of funding",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataFromMillions(spreadsheet, 2, org.RowNumber, 8)
                                                            }
                                                        }
                                                    }
                                                }
                                            },
                                            new FundingLine
                                            {
                                                Name = "Pupil Led funding",
                                                TemplateLineId = templateLineId++,
                                                Value = (GetData(spreadsheet, 2, org.RowNumber, 4) * GetDataInPence(spreadsheet, 2, org.RowNumber, 2)) +
                                                    GetData(spreadsheet, 2, org.RowNumber, 5) * GetDataInPence(spreadsheet, 2, org.RowNumber, 3),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "Primary Pupil funding",
                                                        Type = Enums.CalculationType.PerPupilFunding,
                                                        Value = GetData(spreadsheet, 2, org.RowNumber, 4) * GetDataInPence(spreadsheet, 2, org.RowNumber, 2),
                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                        FormulaText = "= RefData1 * RefData2",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "2019-20 schools block primary pupils (headcount)",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetData(spreadsheet, 2, org.RowNumber, 4)
                                                            },
                                                            new ReferenceData
                                                            {
                                                                Name = "2019-20 schools block primary unit of funding",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataInPence(spreadsheet, 2, org.RowNumber, 2)
                                                            }
                                                        }
                                                    },
                                                    new Calculation
                                                    {
                                                        Name = "Secondary Pupil funding",
                                                        Type = Enums.CalculationType.PerPupilFunding,
                                                        Value = GetData(spreadsheet, 2, org.RowNumber, 5) * GetDataInPence(spreadsheet, 2, org.RowNumber, 3),
                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                        FormulaText = "= RefData1 * RefData2",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "2019-20 schools block secondary pupils (headcount)",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetData(spreadsheet, 2, org.RowNumber, 5)
                                                            },
                                                            new ReferenceData
                                                            {
                                                                Name = "2019-20 schools block secondary unit of funding",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataInPence(spreadsheet, 2, org.RowNumber, 3)
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
                                        Value = GetDataFromMillions(spreadsheet, 1, org.RowNumber, 4),
                                        FundingLines = new List<FundingLine>
                                        {
                                            new FundingLine
                                            {
                                                Name = "CSSB Pupil Led Funding",
                                                Value = GetData(spreadsheet, 2, org.RowNumber, 12) * GetDataInPence(spreadsheet, 2, org.RowNumber, 11),
                                                TemplateLineId = templateLineId++,
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "CSSB Pupil funding",
                                                        Type = Enums.CalculationType.PerPupilFunding,
                                                        Value = GetData(spreadsheet, 2, org.RowNumber, 12) * GetDataInPence(spreadsheet, 2, org.RowNumber, 11),
                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "2019-20 CSSB pupils (headcount)",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetData(spreadsheet, 2, org.RowNumber, 12)
                                                            },
                                                            new ReferenceData
                                                            {
                                                                Name = "2019-20 CSSB unit of funding (£s)",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataInPence(spreadsheet, 2, org.RowNumber, 11)
                                                            }
                                                        }
                                                    }
                                                }
                                            },
                                            new FundingLine
                                            {
                                                Name = "Funding for historic commitments",
                                                TemplateLineId = templateLineId++,
                                                Value = GetDataFromMillions(spreadsheet, 2, org.RowNumber, 13),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "2019-20 CSSB funding for historic commitments",
                                                        Type = Enums.CalculationType.LumpSum,
                                                        Value = GetDataFromMillions(spreadsheet, 2, org.RowNumber, 13),
                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "2019-20 CSSB funding for historic commitments",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataFromMillions(spreadsheet, 2, org.RowNumber, 13)
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
                                        Value = GetDataFromMillions(spreadsheet, 1, org.RowNumber, 5),
                                        FundingLines = new List<FundingLine>
                                        {
                                            new FundingLine
                                            {
                                                Name = "Actual high needs fundings, excluding basic entitelment factor and import/export adjustments",
                                                TemplateLineId = templateLineId++,
                                                Value = GetDataFromMillions(spreadsheet, 3, org.RowNumber, 2),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "HN Block Before Basic Entitlement and Import and Export Adjustments",
                                                        Type = Enums.CalculationType.LumpSum,
                                                        Value = GetDataFromMillions(spreadsheet, 3, org.RowNumber, 2),
                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "HN Block Before Basic Entitlement and Import and Export Adjustments",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataFromMillions(spreadsheet, 3, org.RowNumber, 2)
                                                            }
                                                        }
                                                    }
                                                }
                                            },
                                            new FundingLine
                                            {
                                                Name = "Basic entitlements",
                                                TemplateLineId = templateLineId++,
                                                Value = GetDataInPence(spreadsheet, 3, org.RowNumber, 3) * GetData(spreadsheet, 3, org.RowNumber, 4),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "HN Block Before Basic Entitlement and Import and Export Adjustments",
                                                        Type = Enums.CalculationType.LumpSum,
                                                        Value = GetDataInPence(spreadsheet, 3, org.RowNumber, 3) * GetData(spreadsheet, 3, org.RowNumber, 4),
                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "Basic Entitlement Pupils",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetData(spreadsheet, 3, org.RowNumber, 4)
                                                            },
                                                            new ReferenceData
                                                            {
                                                                Name = "2019-20 ACA-weighted basic entitlement factor unit rate",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataInPence(spreadsheet, 3, org.RowNumber, 3)
                                                            }
                                                        }
                                                    }
                                                }
                                            },
                                            new FundingLine
                                            {
                                                Name = "Import / export adjustments",
                                                TemplateLineId = templateLineId++,
                                                Value = GetDataFromMillions(spreadsheet, 3, org.RowNumber, 5),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "Import/Export Adjustment",
                                                        Type = Enums.CalculationType.LumpSum,
                                                        Value = GetDataFromMillions(spreadsheet, 3, org.RowNumber, 5),
                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "Import/Export Adjustment Rate",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataFromMillions(spreadsheet, 3, org.RowNumber, 5)
                                                            }
                                                        }
                                                    }
                                                }
                                            },
                                            new FundingLine
                                            {
                                                Name = "ONS Population Projection",
                                                TemplateLineId = templateLineId++,
                                                Value = GetData(spreadsheet, 3, org.RowNumber, 6),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "ONS Population Projection",
                                                        Type = Enums.CalculationType.PupilNumber,
                                                        Value = GetData(spreadsheet, 3, org.RowNumber, 6),
                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "Basic Entitlement Pupils",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetData(spreadsheet, 3, org.RowNumber, 6)
                                                            }
                                                        }
                                                    }
                                                }
                                            },
                                            new FundingLine
                                            {
                                                Name = "Additional High Needs Funding",
                                                TemplateLineId = templateLineId++,
                                                Value = GetDataFromMillions(spreadsheet, 3, org.RowNumber, 7),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "Additional High Needs Funding",
                                                        Type = Enums.CalculationType.LumpSum,
                                                        Value = GetDataFromMillions(spreadsheet, 3, org.RowNumber, 7),
                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "Additional High Needs",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataFromMillions(spreadsheet, 3, org.RowNumber, 7)
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
                                        Value = GetDataFromMillions(spreadsheet, 1, org.RowNumber, 6),
                                        FundingLines = new List<FundingLine>
                                        {
                                            new FundingLine
                                            {
                                                Name = "Initial funding funding for universal entitlement for 3 and 4 year olds",
                                                TemplateLineId = templateLineId++,
                                                Value = GetData(spreadsheet, 5, org.RowNumber, 3) *  GetDataInPence(spreadsheet, 5, org.RowNumber, 2) * 15 * 38,
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "HN Block Before Basic Entitlement and Import and Export Adjustments",
                                                        Type = Enums.CalculationType.PerPupilFunding,
                                                        Value = GetData(spreadsheet, 5, org.RowNumber, 3) *  GetDataInPence(spreadsheet, 5, org.RowNumber, 2) * 15 * 38,
                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "Total 3 and 4 Year Olds (PTE)",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetData(spreadsheet, 5, org.RowNumber, 3)
                                                            },
                                                            new ReferenceData
                                                            {
                                                                Name = "Early Years Universal Entitlement for 3 and 4 Year Olds Rate",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataInPence(spreadsheet, 5, org.RowNumber, 2)
                                                            },
                                                            new ReferenceData
                                                            {
                                                                Name = "PTE Funded Hours",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
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
                                                Value = GetData(spreadsheet, 5, org.RowNumber, 5) *  GetDataInPence(spreadsheet, 5, org.RowNumber, 2) * 15 * 38,
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "HN Block Before Basic Entitlement and Import and Export Adjustment",
                                                        Type = Enums.CalculationType.PerPupilFunding,
                                                        Value = GetData(spreadsheet, 5, org.RowNumber, 5) *  GetDataInPence(spreadsheet, 5, org.RowNumber, 2) * 15 * 38,
                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "Total 3 and 4 Year Old for Additional Hours for Working Parents (PTE)",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetData(spreadsheet, 5, org.RowNumber, 5)
                                                            },
                                                            new ReferenceData
                                                            {
                                                                Name = "Early Years Universal Entitlement for 3 and 4 Year Olds Rate",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataInPence(spreadsheet, 5, org.RowNumber, 2)
                                                            },
                                                            new ReferenceData
                                                            {
                                                                Name = "PTE Funded Hours",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
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
                                                Value = GetData(spreadsheet, 5, org.RowNumber, 8) *  GetDataInPence(spreadsheet, 5, org.RowNumber, 7) * 15 * 38,
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "Per Pupil Funding for 2 year old entitlement",
                                                        Type = Enums.CalculationType.PerPupilFunding,
                                                        Value = GetData(spreadsheet, 5, org.RowNumber, 8) *  GetDataInPence(spreadsheet, 5, org.RowNumber, 7) * 15 * 38,
                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "Total 2 Year Olds (PTE)",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetData(spreadsheet, 5, org.RowNumber, 8)
                                                            },
                                                            new ReferenceData
                                                            {
                                                                Name = "LA hourly rate for 2 year old entitlement",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataInPence(spreadsheet, 5, org.RowNumber, 7)
                                                            },
                                                            new ReferenceData
                                                            {
                                                                Name = "PTE Funded Hours",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
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
                                                Value = GetDataFromMillions(spreadsheet, 2, org.RowNumber, 10),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "Early Years Pupil Premium lumpsum",
                                                        Type = Enums.CalculationType.LumpSum,
                                                        Value = GetDataFromMillions(spreadsheet, 2, org.RowNumber, 10),
                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "Early Years Pupil Premium Rate",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
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
                                                Value = GetDataFromMillions(spreadsheet, 2, org.RowNumber, 11),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "Disability Access Fund lumpsum",
                                                        Type = Enums.CalculationType.LumpSum,
                                                        Value = GetDataFromMillions(spreadsheet, 2, org.RowNumber, 11),
                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "Disability Access Fund Rate",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
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
                                                Value = GetDataFromMillions(spreadsheet, 2, org.RowNumber, 12),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "Per Pupil Funding for Maintained Nursery Schools",
                                                        Type = Enums.CalculationType.PerPupilFunding,
                                                        Value = GetDataFromMillions(spreadsheet, 2, org.RowNumber, 12),
                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "Maintained Nursery Schools Supplement (PTE)",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = -1 // Not in the file
                                                            },
                                                            new ReferenceData
                                                            {
                                                                Name = "Maintained Nursery Schools Supplement Hourly Rate",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = -1 // Not in the file
                                                            },
                                                            new ReferenceData
                                                            {
                                                                Name = "PTE Funded Hours",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
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
                                Value = GetDataFromMillions(spreadsheet, 1, org.RowNumber, 11),
                                ProfilePeriods = periods,
                                FundingLines = new List<FundingLine>
                                {
                                    new FundingLine
                                    {
                                        Name = "School Block",
                                        TemplateLineId = templateLineId++,
                                        Type = FundingLineType.Information,
                                        Value = GetDataFromMillions(spreadsheet, 1, org.RowNumber, 7),
                                        FundingLines = new List<FundingLine>
                                        {
                                            new FundingLine
                                            {
                                                Name = "Funding through the mobility and premises factors",
                                                TemplateLineId = templateLineId++,
                                                Value = GetDataFromMillions(spreadsheet, 2, org.RowNumber, 6),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "2019-20 mobility and premises funding",
                                                        Type = Enums.CalculationType.LumpSum,
                                                        Value = GetDataFromMillions(spreadsheet, 2, org.RowNumber, 6),
                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "2019-20 funding through the premises and mobility factors",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataFromMillions(spreadsheet, 2, org.RowNumber, 6)
                                                            }
                                                        }
                                                    }
                                                }
                                            },
                                            new FundingLine
                                            {
                                                Name = "Growth funding",
                                                TemplateLineId = templateLineId++,
                                                Value = GetDataFromMillions(spreadsheet, 2, org.RowNumber, 7),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "2019-20 growth funding",
                                                        Type = Enums.CalculationType.LumpSum,
                                                        Value = GetDataFromMillions(spreadsheet, 2, org.RowNumber, 7),
                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "2019-20 schools block primary unit of funding",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataFromMillions(spreadsheet, 2, org.RowNumber, 7)
                                                            }
                                                        }
                                                    }
                                                }
                                            },
                                            new FundingLine
                                            {
                                                Name = "Pupil Led funding",
                                                TemplateLineId = templateLineId++,
                                                Value = GetDataFromMillions(spreadsheet, 2, org.RowNumber, 2) * GetData(spreadsheet, 2, org.RowNumber, 4),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "2019-20 schools block primary unit of funding",
                                                        Type = Enums.CalculationType.PerPupilFunding,
                                                        Value = GetDataFromMillions(spreadsheet, 2, org.RowNumber, 2) * GetData(spreadsheet, 2, org.RowNumber, 4),
                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "2019-20 schools block primary pupils (headcount)",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetData(spreadsheet, 2, org.RowNumber, 4)
                                                            },
                                                            new ReferenceData
                                                            {
                                                                Name = "2019-20 schools block primary unit of funding",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataFromMillions(spreadsheet, 2, org.RowNumber, 2)
                                                            }
                                                        }
                                                    }
                                                }
                                            },
                                            new FundingLine
                                            {
                                                Name = "Deductions To School Block",
                                                TemplateLineId = templateLineId++,
                                                Value = GetDataFromMillions(spreadsheet, 2, org.RowNumber, 9),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "Deductions To School Block",
                                                        Type = Enums.CalculationType.LumpSum,
                                                        Value =  GetDataFromMillions(spreadsheet, 2, org.RowNumber, 9),
                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "Rate Deductions To School Block",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
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
                                        Value = GetDataFromMillions(spreadsheet, 1, org.RowNumber, 8),
                                        FundingLines = new List<FundingLine>
                                        {
                                            new FundingLine
                                            {
                                                Name = "CSSB Pupil Led Funding",
                                                TemplateLineId = templateLineId++,
                                                Value = GetDataInPence(spreadsheet, 2, org.RowNumber, 11) * GetData(spreadsheet, 2, org.RowNumber, 12),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "Rate Deductions To School Block",
                                                        Type = Enums.CalculationType.PerPupilFunding,
                                                        Value = GetDataInPence(spreadsheet, 2, org.RowNumber, 11) * GetData(spreadsheet, 2, org.RowNumber, 12),
                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "2019-20 CSSB pupils (headcount)",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetData(spreadsheet, 2, org.RowNumber, 12)
                                                            },
                                                            new ReferenceData
                                                            {
                                                                Name = "2019-20 CSSB unit of funding (£s)",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataInPence(spreadsheet, 2, org.RowNumber, 11)
                                                            }
                                                        }
                                                    }
                                                }
                                            },
                                            new FundingLine
                                            {
                                                Name = "Funding for historic commitments",
                                                Value = GetDataFromMillions(spreadsheet, 2, org.RowNumber, 13),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "2019-20 CSSB funding for historic commitments",
                                                        Type = Enums.CalculationType.LumpSum,
                                                        Value = GetDataFromMillions(spreadsheet, 2, org.RowNumber, 13),
                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "2019-20 CSSB funding for historic commitments",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataFromMillions(spreadsheet, 2, org.RowNumber, 13)
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
                                        Value = GetDataFromMillions(spreadsheet, 1, org.RowNumber, 9),
                                        FundingLines = new List<FundingLine>
                                        {
                                            new FundingLine
                                            {
                                                Name = "Actual high needs fundings, excluding basic entitelment factor and import/export adjustments",
                                                Value = GetDataFromMillions(spreadsheet, 3, org.RowNumber, 2),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "HN Block Before Basic Entitlement and Import and Export Adjustments",
                                                        Type = Enums.CalculationType.LumpSum,
                                                        Value = GetDataFromMillions(spreadsheet, 3, org.RowNumber, 2),
                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "HN Block Before Basic Entitlement and Import and Export Adjustments",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataFromMillions(spreadsheet, 3, org.RowNumber, 2)
                                                            }
                                                        }
                                                    }
                                                }
                                            },
                                            new FundingLine
                                            {
                                                Name = "Basic entitlements",
                                                Value = GetData(spreadsheet, 3, org.RowNumber, 4) * GetDataInPence(spreadsheet, 3, org.RowNumber, 3),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "HN Block Before Basic Entitlement and Import and Export Adjustments",
                                                        Type = Enums.CalculationType.PerPupilFunding,
                                                        Value = GetData(spreadsheet, 3, org.RowNumber, 4) * GetDataInPence(spreadsheet, 3, org.RowNumber, 3),
                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "Basic Entitlement Pupils",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetData(spreadsheet, 3, org.RowNumber, 4)
                                                            },
                                                            new ReferenceData
                                                            {
                                                                Name = "2019-20 ACA-weighted basic entitlement factor unit rate",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataInPence(spreadsheet, 3, org.RowNumber, 3)
                                                            }
                                                        }
                                                    }
                                                }
                                            },
                                            new FundingLine
                                            {
                                                Name = "Import / export adjustments",
                                                Value = GetDataFromMillions(spreadsheet, 3, org.RowNumber, 5),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "Import/Export Adjustment",
                                                        Type = Enums.CalculationType.LumpSum,
                                                        Value = GetDataFromMillions(spreadsheet, 3, org.RowNumber, 5),
                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "Import/Export Adjustment Rate",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
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
                                                Value = GetData(spreadsheet, 3, org.RowNumber, 6),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "ONS Population Projection",
                                                        Type = Enums.CalculationType.PupilNumber,
                                                        Value = GetData(spreadsheet, 3, org.RowNumber, 6),
                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "Basic Entitlement Pupils",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetData(spreadsheet, 3, org.RowNumber, 6),
                                                            }
                                                        }
                                                    }
                                                }
                                            },
                                            new FundingLine
                                            {
                                                Name = "Additional High Needs Funding",
                                                Value = GetDataFromMillions(spreadsheet, 3, org.RowNumber, 7),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "Additional High Needs Funding",
                                                        Type = Enums.CalculationType.LumpSum,
                                                        Value = GetDataFromMillions(spreadsheet, 3, org.RowNumber, 7),
                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "Additional High Needs",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataFromMillions(spreadsheet, 3, org.RowNumber, 7)
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
                                                        Value = GetDataFromMillions(spreadsheet, 4, org.RowNumber, 4) + GetDataFromMillions(spreadsheet, 4, org.RowNumber, 7)
                                                            + GetDataFromMillions(spreadsheet, 4, org.RowNumber, 10) + GetDataFromMillions(spreadsheet, 4, org.RowNumber, 13),
                                                        FundingLines = new List<FundingLine>
                                                        {
                                                            new FundingLine
                                                            {
                                                                Name = "Pre-16 SEN Places @ £6k",
                                                                Value = GetDataFromMillions(spreadsheet, 4, org.RowNumber, 4),
                                                                Calculations = new List<Calculation>
                                                                {
                                                                    new Calculation
                                                                    {
                                                                        Name = "April 2019-August 2019",
                                                                        Type = Enums.CalculationType.PerPupilFunding,
                                                                        Value = GetData(spreadsheet, 4, org.RowNumber, 2) * 600000,
                                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                                        FormulaText = "",
                                                                        TemplateCalculationId = templateCalculationId++,
                                                                        ReferenceData = new List<ReferenceData>
                                                                        {
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "April 2019-August 2020 Pupil Number",
                                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = GetData(spreadsheet, 4, org.RowNumber, 2)
                                                                            },
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "April 2019-August 2020 Rate SEN/AP",
                                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = 600000
                                                                            }
                                                                        }
                                                                    },
                                                                    new Calculation
                                                                    {
                                                                        Name = "Sept 2019-March 2020",
                                                                        Type = Enums.CalculationType.PerPupilFunding,
                                                                        Value = GetData(spreadsheet, 4, org.RowNumber, 3) * 600000,
                                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                                        FormulaText = "",
                                                                        TemplateCalculationId = templateCalculationId++,
                                                                        ReferenceData = new List<ReferenceData>
                                                                        {
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "Sept 2019-March 2020 Pupil Number",
                                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = GetData(spreadsheet, 4, org.RowNumber, 3)
                                                                            },
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "Sept 2019-March 2020 Rate SEN/AP",
                                                                                Format = Enums.ReferenceDataValueFormat.Number,
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
                                                                Value = GetDataFromMillions(spreadsheet, 4, org.RowNumber, 7),
                                                                Calculations = new List<Calculation>
                                                                {
                                                                    new Calculation
                                                                    {
                                                                        Name = "April 2019-August 2019",
                                                                        Type = Enums.CalculationType.PerPupilFunding,
                                                                        Value = GetData(spreadsheet, 4, org.RowNumber, 5) * 1000000,
                                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                                        FormulaText = "",
                                                                        TemplateCalculationId = templateCalculationId++,
                                                                        ReferenceData = new List<ReferenceData>
                                                                        {
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "April 2019-August 2020 Pupil Number",
                                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = GetData(spreadsheet, 4, org.RowNumber, 5)
                                                                            },
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "April 2019-August 2020 Rate SEN/AP",
                                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = 1000000
                                                                            }
                                                                        }
                                                                    },
                                                                    new Calculation
                                                                    {
                                                                        Name = "Sept 2019-March 2020",
                                                                        Type = Enums.CalculationType.PerPupilFunding,
                                                                        Value = GetData(spreadsheet, 4, org.RowNumber, 6) * 1000000,
                                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                                        FormulaText = "",
                                                                        TemplateCalculationId = templateCalculationId++,
                                                                        ReferenceData = new List<ReferenceData>
                                                                        {
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "Sept 2019-March 2020 Pupil Number",
                                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = GetData(spreadsheet, 4, org.RowNumber, 6)
                                                                            },
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "Sept 2019-March 2020 Rate SEN/AP",
                                                                                Format = Enums.ReferenceDataValueFormat.Number,
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
                                                                Value = GetDataFromMillions(spreadsheet, 4, org.RowNumber, 10),
                                                                Calculations = new List<Calculation>
                                                                {
                                                                    new Calculation
                                                                    {
                                                                        Name = "April 2019-August 2019",
                                                                        Type = Enums.CalculationType.PerPupilFunding,
                                                                        Value = GetData(spreadsheet, 4, org.RowNumber, 8) * 240000,
                                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                                        FormulaText = "",
                                                                        TemplateCalculationId = templateCalculationId++,
                                                                        ReferenceData = new List<ReferenceData>
                                                                        {
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "April 2019-August 2020 Pupil Number",
                                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = GetData(spreadsheet, 4, org.RowNumber, 8)
                                                                            },
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "April 2019-August 2020 Rate SEN/AP",
                                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = 240000 // I think
                                                                            }
                                                                        }
                                                                    },
                                                                    new Calculation
                                                                    {
                                                                        Name = "Sept 2019-March 2020",
                                                                        Type = Enums.CalculationType.PerPupilFunding,
                                                                        Value = GetData(spreadsheet, 4, org.RowNumber, 9) * 240000,
                                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                                        FormulaText = "",
                                                                        TemplateCalculationId = templateCalculationId++,
                                                                        ReferenceData = new List<ReferenceData>
                                                                        {
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "Sept 2019-March 2020 Pupil Number",
                                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = GetData(spreadsheet, 4, org.RowNumber, 9)
                                                                            },
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "Sept 2019-March 2020 Rate SEN/AP",
                                                                                Format = Enums.ReferenceDataValueFormat.Number,
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
                                                                Value = GetDataFromMillions(spreadsheet, 4, org.RowNumber, 13),
                                                                Calculations = new List<Calculation>
                                                                {
                                                                    new Calculation
                                                                    {
                                                                        Name = "April 2019-August 2019",
                                                                        Type = Enums.CalculationType.PerPupilFunding,
                                                                        Value = GetData(spreadsheet, 4, org.RowNumber, 11) * -1,
                                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                                        FormulaText = "",
                                                                        TemplateCalculationId = templateCalculationId++,
                                                                        ReferenceData = new List<ReferenceData>
                                                                        {
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "April 2019-August 2020 Pupil Number",
                                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = GetData(spreadsheet, 4, org.RowNumber, 11)
                                                                            },
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "April 2019-August 2020 Rate SEN/AP",
                                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = -1 // Not given in file, and I can't infer
                                                                            }
                                                                        }
                                                                    },
                                                                    new Calculation
                                                                    {
                                                                        Name = "Sept 2019-March 2020",
                                                                        Type = Enums.CalculationType.PerPupilFunding,
                                                                        Value = GetData(spreadsheet, 4, org.RowNumber, 12) * -1,
                                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                                        FormulaText = "",
                                                                        TemplateCalculationId = templateCalculationId++,
                                                                        ReferenceData = new List<ReferenceData>
                                                                        {
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "Sept 2019-March 2020 Pupil Number",
                                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = GetData(spreadsheet, 4, org.RowNumber, 12)
                                                                            },
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "Sept 2019-March 2020 Rate SEN/AP",
                                                                                Format = Enums.ReferenceDataValueFormat.Number,
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
                                                        Value = GetDataFromMillions(spreadsheet, 4, org.RowNumber, 16) + GetDataFromMillions(spreadsheet, 4, org.RowNumber, 19)
                                                            + GetDataFromMillions(spreadsheet, 4, org.RowNumber, 22),
                                                        FundingLines = new List<FundingLine>
                                                        {
                                                            new FundingLine
                                                            {
                                                                Name = "Pre-16 SEN Places",
                                                                Value = GetDataFromMillions(spreadsheet, 4, org.RowNumber, 16),
                                                                Calculations = new List<Calculation>
                                                                {
                                                                    new Calculation
                                                                    {
                                                                        Name = "April 2019-August 2019",
                                                                        Type = Enums.CalculationType.PerPupilFunding,
                                                                        Value = GetData(spreadsheet, 4, org.RowNumber, 14) * -1,
                                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                                        FormulaText = "",
                                                                        TemplateCalculationId = templateCalculationId++,
                                                                        ReferenceData = new List<ReferenceData>
                                                                        {
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "April 2019-August 2020 Pupil Number",
                                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = GetData(spreadsheet, 4, org.RowNumber, 14)
                                                                            },
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "April 2019-August 2020 Rate SEN/AP",
                                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = -1 // Not given
                                                                            }
                                                                        }
                                                                    },
                                                                    new Calculation
                                                                    {
                                                                        Name = "Sept 2019-March 2020",
                                                                        Type = Enums.CalculationType.PerPupilFunding,
                                                                        Value = GetData(spreadsheet, 4, org.RowNumber, 15) * -1,
                                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                                        FormulaText = "",
                                                                        TemplateCalculationId = templateCalculationId++,
                                                                        ReferenceData = new List<ReferenceData>
                                                                        {
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "Sept 2019-March 2020 Pupil Number",
                                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = GetData(spreadsheet, 4, org.RowNumber, 15)
                                                                            },
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "Sept 2019-March 2020 Rate SEN/AP",
                                                                                Format = Enums.ReferenceDataValueFormat.Number,
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
                                                                Value = GetDataFromMillions(spreadsheet, 4, org.RowNumber, 19),
                                                                Calculations = new List<Calculation>
                                                                {
                                                                    new Calculation
                                                                    {
                                                                        Name = "April 2019-August 2019",
                                                                        Type = Enums.CalculationType.PerPupilFunding,
                                                                        Value = GetData(spreadsheet, 4, org.RowNumber, 17) * -1,
                                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                                        FormulaText = "",
                                                                        TemplateCalculationId = templateCalculationId++,
                                                                        ReferenceData = new List<ReferenceData>
                                                                        {
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "April 2019-August 2020 Pupil Number",
                                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = GetData(spreadsheet, 4, org.RowNumber, 17)
                                                                            },
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "April 2019-August 2020 Rate SEN/AP",
                                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = -1 // Not given
                                                                            }

                                                                        }
                                                                    },
                                                                    new Calculation
                                                                    {
                                                                        Name = "Sept 2019-March 2020",
                                                                        Type = Enums.CalculationType.PerPupilFunding,
                                                                        Value = GetData(spreadsheet, 4, org.RowNumber, 18) * -1,
                                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                                        FormulaText = "",
                                                                        TemplateCalculationId = templateCalculationId++,
                                                                        ReferenceData = new List<ReferenceData>
                                                                        {
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "Sept 2019-March 2020 Pupil Number",
                                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = GetData(spreadsheet, 4, org.RowNumber, 18)
                                                                            },
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "Sept 2019-March 2020 Rate SEN/AP",
                                                                                Format = Enums.ReferenceDataValueFormat.Number,
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
                                                                Value = GetDataFromMillions(spreadsheet, 4, org.RowNumber, 22),
                                                                Calculations = new List<Calculation>
                                                                {
                                                                    new Calculation
                                                                    {
                                                                        Name = "April 2019-August 2019",
                                                                        Type = Enums.CalculationType.PerPupilFunding,
                                                                        Value = GetData(spreadsheet, 4, org.RowNumber, 20) * -1,
                                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                                        FormulaText = "",
                                                                        TemplateCalculationId = templateCalculationId++,
                                                                        ReferenceData = new List<ReferenceData>
                                                                        {
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "April 2019-August 2020 Pupil Number",
                                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = GetData(spreadsheet, 4, org.RowNumber, 20)
                                                                            },
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "April 2019-August 2020 Rate SEN/AP",
                                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = -1 // Not given
                                                                            }
                                                                        }
                                                                    },
                                                                    new Calculation
                                                                    {
                                                                        Name = "Sept 2019-March 2020",
                                                                        Type = Enums.CalculationType.PerPupilFunding,
                                                                        Value = GetData(spreadsheet, 4, org.RowNumber, 21) * -1,
                                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                                        FormulaText = "",
                                                                        TemplateCalculationId = templateCalculationId++,
                                                                        ReferenceData = new List<ReferenceData>
                                                                        {
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "Sept 2019-March 2020 Pupil Number",
                                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = GetData(spreadsheet, 4, org.RowNumber, 21)
                                                                            },
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "Sept 2019-March 2020 Rate SEN/AP",
                                                                                Format = Enums.ReferenceDataValueFormat.Number,
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
                                                        Value = GetDataFromMillions(spreadsheet, 4, org.RowNumber, 25) + GetDataFromMillions(spreadsheet, 4, org.RowNumber, 28),
                                                        FundingLines = new List<FundingLine>
                                                        {
                                                            new FundingLine
                                                            {
                                                                Name = "Pre-16 SEN Places",
                                                                Value = GetDataFromMillions(spreadsheet, 4, org.RowNumber, 25),
                                                                Calculations = new List<Calculation>
                                                                {
                                                                    new Calculation
                                                                    {
                                                                        Name = "April 2019-August 2019",
                                                                        Type = Enums.CalculationType.PerPupilFunding,
                                                                        Value = GetData(spreadsheet, 4, org.RowNumber, 23) * -1,
                                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                                        FormulaText = "",
                                                                        TemplateCalculationId = templateCalculationId++,
                                                                        ReferenceData = new List<ReferenceData>
                                                                        {
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "April 2019-August 2020 Pupil Number",
                                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = GetData(spreadsheet, 4, org.RowNumber, 23)
                                                                            },
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "April 2019-August 2020 Rate SEN/AP",
                                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = -1 // Not given
                                                                            }
                                                                        }
                                                                    },
                                                                    new Calculation
                                                                    {
                                                                        Name = "Sept 2019-March 2020",
                                                                        Type = Enums.CalculationType.PerPupilFunding,
                                                                        Value = GetData(spreadsheet, 4, org.RowNumber, 24) * -1,
                                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                                        FormulaText = "",
                                                                        TemplateCalculationId = templateCalculationId++,
                                                                        ReferenceData = new List<ReferenceData>
                                                                        {
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "Sept 2019-March 2020 Pupil Number",
                                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = GetData(spreadsheet, 4, org.RowNumber, 24)
                                                                            },
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "Sept 2019-March 2020 Rate SEN/AP",
                                                                                Format = Enums.ReferenceDataValueFormat.Number,
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
                                                                Value = GetDataFromMillions(spreadsheet, 4, org.RowNumber, 28),
                                                                Calculations = new List<Calculation>
                                                                {
                                                                    new Calculation
                                                                    {
                                                                        Name = "April 2019-August 2019",
                                                                        Type = Enums.CalculationType.PerPupilFunding,
                                                                        Value = GetData(spreadsheet, 4, org.RowNumber, 26) * -1,
                                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                                        FormulaText = "",
                                                                        TemplateCalculationId = templateCalculationId++,
                                                                        ReferenceData = new List<ReferenceData>
                                                                        {
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "April 2019-August 2020 Pupil Number",
                                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = GetData(spreadsheet, 4, org.RowNumber, 26)
                                                                            },
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "April 2019-August 2020 Rate SEN/AP",
                                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = -1 // Not given
                                                                            }
                                                                        }
                                                                    },
                                                                    new Calculation
                                                                    {
                                                                        Name = "Sept 2019-March 2020",
                                                                        Type = Enums.CalculationType.PerPupilFunding,
                                                                        Value = GetData(spreadsheet, 4, org.RowNumber, 27) * -1,
                                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                                        FormulaText = "",
                                                                        TemplateCalculationId = templateCalculationId++,
                                                                        ReferenceData = new List<ReferenceData>
                                                                        {
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "Sept 2019-March 2020 Pupil Numbe",
                                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = GetData(spreadsheet, 4, org.RowNumber, 27)
                                                                            },
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "Sept 2019-March 2020 Rate SEN/AP",
                                                                                Format = Enums.ReferenceDataValueFormat.Number,
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
                                                                Value = GetDataFromMillions(spreadsheet, 4, org.RowNumber, 31),
                                                                Calculations = new List<Calculation>
                                                                {
                                                                    new Calculation
                                                                    {
                                                                        Name = "April 2019-August 2019",
                                                                        Type = Enums.CalculationType.PerPupilFunding,
                                                                        Value = GetData(spreadsheet, 4, org.RowNumber, 29) * -1,
                                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                                        FormulaText = "",
                                                                        TemplateCalculationId = templateCalculationId++,
                                                                        ReferenceData = new List<ReferenceData>
                                                                        {
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "April 2019-July 2020 Pupil Number",
                                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = GetData(spreadsheet, 4, org.RowNumber, 29)
                                                                            },
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "April 2019-July 2020 Rate SEN/AP",
                                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = -1 // Not given
                                                                            }
                                                                        }
                                                                    },
                                                                    new Calculation
                                                                    {
                                                                        Name = "Sept 2019-March 2020",
                                                                        Type = Enums.CalculationType.PerPupilFunding,
                                                                        Value = GetData(spreadsheet, 4, org.RowNumber, 30) * -1,
                                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                                        FormulaText = "",
                                                                        TemplateCalculationId = templateCalculationId++,
                                                                        ReferenceData = new List<ReferenceData>
                                                                        {
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "Aug 2019-March 2020 Pupil Number",
                                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = GetData(spreadsheet, 4, org.RowNumber, 30)
                                                                            },
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "Aug 2019-March 2020 Rate SEN/AP",
                                                                                Format = Enums.ReferenceDataValueFormat.Number,
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
                                                                Value = GetDataFromMillions(spreadsheet, 4, org.RowNumber, 34),
                                                                Calculations = new List<Calculation>
                                                                {
                                                                    new Calculation
                                                                    {
                                                                        Name = "April 2019-August 2019",
                                                                        Type = Enums.CalculationType.PerPupilFunding,
                                                                        Value = GetData(spreadsheet, 4, org.RowNumber, 32) * -1,
                                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                                        FormulaText = "",
                                                                        TemplateCalculationId = templateCalculationId++,
                                                                        ReferenceData = new List<ReferenceData>
                                                                        {
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "April 2019-July 2020 Pupil Number",
                                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = GetData(spreadsheet, 4, org.RowNumber, 32)
                                                                            },
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "April 2019-July 2020 Rate SEN/AP",
                                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = -1 // Not given
                                                                            }
                                                                        }
                                                                    },
                                                                    new Calculation
                                                                    {
                                                                        Name = "Sept 2019-March 2020",
                                                                        Type = Enums.CalculationType.PerPupilFunding,
                                                                        Value = GetData(spreadsheet, 4, org.RowNumber, 33) * -1,
                                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                                        FormulaText = "",
                                                                        TemplateCalculationId = templateCalculationId++,
                                                                        ReferenceData = new List<ReferenceData>
                                                                        {
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "Aug 2019-March 2020 Pupil Number",
                                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                                TemplateReferenceId = templateReferenceId++,
                                                                                Value = GetData(spreadsheet, 4, org.RowNumber, 33)
                                                                            },
                                                                            new ReferenceData
                                                                            {
                                                                                Name = "Aug 2019-March 2020 Rate SEN/AP",
                                                                                Format = Enums.ReferenceDataValueFormat.Number,
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
                                                        Value = GetDataFromMillions(spreadsheet, 4, org.RowNumber, 37),
                                                        Calculations = new List<Calculation>
                                                        {
                                                            new Calculation
                                                            {
                                                                Name = "April 2019-August 2019",
                                                                Type = Enums.CalculationType.PerPupilFunding,
                                                                Value = GetData(spreadsheet, 4, org.RowNumber, 35) * -1,
                                                                ValueFormat = Enums.CalculationValueFormat.Currency,
                                                                FormulaText = "",
                                                                TemplateCalculationId = templateCalculationId++,
                                                                ReferenceData = new List<ReferenceData>
                                                                {
                                                                    new ReferenceData
                                                                    {
                                                                        Name = "April 2019-July 2020 Pupil Number",
                                                                        Format = Enums.ReferenceDataValueFormat.Number,
                                                                        TemplateReferenceId = templateReferenceId++,
                                                                        Value = GetData(spreadsheet, 4, org.RowNumber, 35)
                                                                    },
                                                                    new ReferenceData
                                                                    {
                                                                        Name = "April 2019-July 2020 Rate SEN/AP",
                                                                        Format = Enums.ReferenceDataValueFormat.Number,
                                                                        TemplateReferenceId = templateReferenceId++,
                                                                        Value = -1 // Not given
                                                                    }
                                                                }
                                                            },
                                                            new Calculation
                                                            {
                                                                Name = "Sept 2019-March 2020",
                                                                Type = Enums.CalculationType.PerPupilFunding,
                                                                Value = GetData(spreadsheet, 4, org.RowNumber, 36) * -1,
                                                                ValueFormat = Enums.CalculationValueFormat.Currency,
                                                                FormulaText = "",
                                                                TemplateCalculationId = templateCalculationId++,
                                                                ReferenceData = new List<ReferenceData>
                                                                {
                                                                    new ReferenceData
                                                                    {
                                                                        Name = "Aug 2019-March 2020 Pupil Number",
                                                                        Format = Enums.ReferenceDataValueFormat.Number,
                                                                        TemplateReferenceId = templateReferenceId++,
                                                                        Value = GetData(spreadsheet, 4, org.RowNumber, 36)
                                                                    },
                                                                    new ReferenceData
                                                                    {
                                                                        Name = "Aug 2019-March 2020 Rate SEN/AP",
                                                                        Format = Enums.ReferenceDataValueFormat.Number,
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
                                                        Value = GetDataFromMillions(spreadsheet, 4, org.RowNumber, 40),
                                                        Calculations = new List<Calculation>
                                                        {
                                                            new Calculation
                                                            {
                                                                Name = "April 2019-August 2019",
                                                                Type = Enums.CalculationType.PerPupilFunding,
                                                                Value = GetData(spreadsheet, 4, org.RowNumber, 38) * -1,
                                                                ValueFormat = Enums.CalculationValueFormat.Currency,
                                                                FormulaText = "",
                                                                TemplateCalculationId = templateCalculationId++,
                                                                ReferenceData = new List<ReferenceData>
                                                                {
                                                                    new ReferenceData
                                                                    {
                                                                        Name = "April 2019-July 2020 Pupil Number",
                                                                        Format = Enums.ReferenceDataValueFormat.Number,
                                                                        TemplateReferenceId = templateReferenceId++,
                                                                        Value = GetData(spreadsheet, 4, org.RowNumber, 38)
                                                                    },
                                                                    new ReferenceData
                                                                    {
                                                                        Name = "April 2019-July 2020 Rate SEN/AP",
                                                                        Format = Enums.ReferenceDataValueFormat.Number,
                                                                        TemplateReferenceId = templateReferenceId++,
                                                                        Value = -1 // Not given
                                                                    }
                                                                }
                                                            },
                                                            new Calculation
                                                            {
                                                                Name = "Sept 2019-March 2020",
                                                                Type = Enums.CalculationType.PerPupilFunding,
                                                                Value = GetData(spreadsheet, 4, org.RowNumber, 39) * -1,
                                                                ValueFormat = Enums.CalculationValueFormat.Currency,
                                                                FormulaText = "",
                                                                TemplateCalculationId = templateCalculationId++,
                                                                ReferenceData = new List<ReferenceData>
                                                                {
                                                                    new ReferenceData
                                                                    {
                                                                        Name = "Aug 2019-March 2020 Pupil Number",
                                                                        Format = Enums.ReferenceDataValueFormat.Number,
                                                                        TemplateReferenceId = templateReferenceId++,
                                                                        Value = GetData(spreadsheet, 4, org.RowNumber, 39)
                                                                    },
                                                                    new ReferenceData
                                                                    {
                                                                        Name = "Aug 2019-March 2020 Rate SEN/AP",
                                                                        Format = Enums.ReferenceDataValueFormat.Number,
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
                                                        Value = GetDataFromMillions(spreadsheet, 4, org.RowNumber, 43),
                                                        Calculations = new List<Calculation>
                                                        {
                                                            new Calculation
                                                            {
                                                                Name = "April 2019-August 2019",
                                                                Type = Enums.CalculationType.LumpSum,
                                                                Value = GetData(spreadsheet, 4, org.RowNumber, 41) * -1,
                                                                ValueFormat = Enums.CalculationValueFormat.Currency,
                                                                FormulaText = "",
                                                                TemplateCalculationId = templateCalculationId++,
                                                                ReferenceData = new List<ReferenceData>
                                                                {
                                                                    new ReferenceData
                                                                    {
                                                                        Name = "April 2019-July 2020 Pupil Number",
                                                                        Format = Enums.ReferenceDataValueFormat.Number,
                                                                        TemplateReferenceId = templateReferenceId++,
                                                                        Value = GetData(spreadsheet, 4, org.RowNumber, 41)
                                                                    },
                                                                    new ReferenceData
                                                                    {
                                                                        Name = "April 2019-July 2020 Rate SEN/AP",
                                                                        Format = Enums.ReferenceDataValueFormat.Number,
                                                                        TemplateReferenceId = templateReferenceId++,
                                                                        Value = -1 // Not given
                                                                    }
                                                                }
                                                            },
                                                            new Calculation
                                                            {
                                                                Name = "Sept 2019-March 2020",
                                                                Type = Enums.CalculationType.LumpSum,
                                                                Value = GetData(spreadsheet, 4, org.RowNumber, 42) * -1,
                                                                ValueFormat = Enums.CalculationValueFormat.Currency,
                                                                FormulaText = "",
                                                                TemplateCalculationId = templateCalculationId++,
                                                                ReferenceData = new List<ReferenceData>
                                                                {
                                                                    new ReferenceData
                                                                    {
                                                                        Name = "Aug 2019-March 2020 Pupil Number",
                                                                        Format = Enums.ReferenceDataValueFormat.Number,
                                                                        TemplateReferenceId = templateReferenceId++,
                                                                        Value = GetData(spreadsheet, 4, org.RowNumber, 42)
                                                                    },
                                                                    new ReferenceData
                                                                    {
                                                                        Name = "Aug 2019-March 2020 Rate SEN/AP",
                                                                        Format = Enums.ReferenceDataValueFormat.Number,
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
                                        Value = GetDataFromMillions(spreadsheet, 1, org.RowNumber, 10),
                                        FundingLines = new List<FundingLine>
                                        {
                                            new FundingLine
                                            {
                                                Name = "Initial funding funding for universal entitlement for 3 and 4 year olds",
                                                Value = GetDataFromMillions(spreadsheet, 5, org.RowNumber, 4),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "HN Block Before Basic Entitlement and Import and Export Adjustments",
                                                        Type = Enums.CalculationType.PerPupilFunding,
                                                        Value = GetDataFromMillions(spreadsheet, 5, org.RowNumber, 4),
                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "Total 3 and 4 Year Old for Additional Hours for Working Parents (PTE)",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetData(spreadsheet, 5, org.RowNumber, 3)
                                                            },
                                                            new ReferenceData
                                                            {
                                                                Name = "Early Years Universal Entitlement for 3 and 4 Year Olds Rate",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataInPence(spreadsheet, 5, org.RowNumber, 2)
                                                            },
                                                            new ReferenceData
                                                            {
                                                                Name = "PTE Funded Hours",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
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
                                                Value = GetDataFromMillions(spreadsheet, 5, org.RowNumber, 6),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "HN Block Before Basic Entitlement and Import and Export Adjustments",
                                                        Type = Enums.CalculationType.PerPupilFunding,
                                                        Value = GetDataFromMillions(spreadsheet, 5, org.RowNumber, 6),
                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "Total 3 and 4 Year Old for Additional Hours for Working Parents (PTE)",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetData(spreadsheet, 5, org.RowNumber, 5)
                                                            },
                                                            new ReferenceData
                                                            {
                                                                Name = "Early Years Universal Entitlement for 3 and 4 Year Olds Rate",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataInPence(spreadsheet, 5, org.RowNumber, 2)
                                                            },
                                                            new ReferenceData
                                                            {
                                                                Name = "PTE Funded Hours",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
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
                                                Value = GetDataFromMillions(spreadsheet, 5, org.RowNumber, 9),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "Per Pupil Funding for 2 year old entitlement",
                                                        Type = Enums.CalculationType.LumpSum,
                                                        Value =  GetDataFromMillions(spreadsheet, 5, org.RowNumber, 9),
                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "Total 2 Year Olds (PTE)",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetData(spreadsheet, 5, org.RowNumber, 8)
                                                            },
                                                            new ReferenceData
                                                            {
                                                                Name = "LA hourly rate for 2 year old entitlement",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = GetDataInPence(spreadsheet, 5, org.RowNumber, 7)
                                                            },
                                                            new ReferenceData
                                                            {
                                                                Name = "PTE Funded Hours",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
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
                                                Value = GetDataFromMillions(spreadsheet, 5, org.RowNumber, 10),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "Early Years Pupil Premium lumpsum",
                                                        Type = Enums.CalculationType.LumpSum,
                                                        Value = GetDataFromMillions(spreadsheet, 5, org.RowNumber, 10),
                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "Early Years Pupil Premium Rate",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
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
                                                Value = GetDataFromMillions(spreadsheet, 5, org.RowNumber, 11),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "Disability Access Fund lumpsum",
                                                        Type = Enums.CalculationType.LumpSum,
                                                        Value = GetDataFromMillions(spreadsheet, 5, org.RowNumber, 11),
                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "Disability Access Fund Rate",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
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
                                                Value = GetDataFromMillions(spreadsheet, 5, org.RowNumber, 12),
                                                Calculations = new List<Calculation>
                                                {
                                                    new Calculation
                                                    {
                                                        Name = "Per Pupil Funding for Maintained Nursery Schools",
                                                        Type = Enums.CalculationType.PerPupilFunding,
                                                        Value = GetDataFromMillions(spreadsheet, 5, org.RowNumber, 12),
                                                        ValueFormat = Enums.CalculationValueFormat.Currency,
                                                        FormulaText = "",
                                                        TemplateCalculationId = templateCalculationId++,
                                                        ReferenceData = new List<ReferenceData>
                                                        {
                                                            new ReferenceData
                                                            {
                                                                Name = "Maintained Nursery Schools Supplement (PTE)",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = -1
                                                            },
                                                            new ReferenceData
                                                            {
                                                                Name = "Maintained Nursery Schools Supplement Hourly Rate",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
                                                                TemplateReferenceId = templateReferenceId++,
                                                                Value = -1
                                                            },
                                                            new ReferenceData
                                                            {
                                                                Name = "PTE Funded Hours",
                                                                Format = Enums.ReferenceDataValueFormat.Number,
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
                        }
                    }
                }
            };
        }

        private static List<FundingLinePeriod> GetPeriods(long periodValue)
        {
            return new List<FundingLinePeriod>
            {
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    PeriodCode = "FY1920",
                    Year = 2019,
                    TypeValue = "April",
                    Occurence = 1,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    PeriodCode = "FY1920",
                    Year = 2019,
                    TypeValue = "April",
                    Occurence = 2,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    PeriodCode = "FY1920",
                    Year = 2019,
                    TypeValue = "April",
                    Occurence = 3,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    PeriodCode = "FY1920",
                    Year = 2019,
                    TypeValue = "May",
                    Occurence = 1,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    PeriodCode = "FY1920",
                    Year = 2019,
                    TypeValue = "May",
                    Occurence = 2,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    PeriodCode = "FY1920",
                    Year = 2019,
                    TypeValue = "June",
                    Occurence = 1,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    PeriodCode = "FY1920",
                    Year = 2019,
                    TypeValue = "June",
                    Occurence = 2,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    PeriodCode = "FY1920",
                    Year = 2019,
                    TypeValue = "July",
                    Occurence = 1,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    PeriodCode = "FY1920",
                    Year = 2019,
                    TypeValue = "July",
                    Occurence = 2,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    PeriodCode = "FY1920",
                    Year = 2019,
                    TypeValue = "August",
                    Occurence = 1,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    PeriodCode = "FY1920",
                    Year = 2019,
                    TypeValue = "August",
                    Occurence = 2,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    PeriodCode = "FY1920",
                    Year = 2019,
                    TypeValue = "September",
                    Occurence = 1,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    PeriodCode = "FY1920",
                    Year = 2019,
                    TypeValue = "September",
                    Occurence = 2,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    PeriodCode = "FY1920",
                    Year = 2019,
                    TypeValue = "October",
                    Occurence = 1,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    PeriodCode = "FY1920",
                    Year = 2019,
                    TypeValue = "October",
                    Occurence = 2,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    PeriodCode = "FY1920",
                    Year = 2019,
                    TypeValue = "November",
                    Occurence = 1,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    PeriodCode = "FY1920",
                    Year = 2019,
                    TypeValue = "November",
                    Occurence = 2,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    PeriodCode = "FY1920",
                    Year = 2019,
                    TypeValue = "December",
                    Occurence = 1,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    PeriodCode = "FY1920",
                    Year = 2019,
                    TypeValue = "December",
                    Occurence = 2,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    PeriodCode = "FY1920",
                    Year = 2020,
                    TypeValue = "January",
                    Occurence = 1,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    PeriodCode = "FY1920",
                    Year = 2020,
                    TypeValue = "January",
                    Occurence = 2,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    PeriodCode = "FY1920",
                    Year = 2020,
                    TypeValue = "Febuary",
                    Occurence = 1,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    PeriodCode = "FY1920",
                    Year = 2020,
                    TypeValue = "Febuary",
                    Occurence = 2,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    PeriodCode = "FY1920",
                    Year = 2020,
                    TypeValue = "March",
                    Occurence = 1,
                    ProfiledValue = periodValue
                },
                new FundingLinePeriod
                {
                    Type = FundingLinePeriodType.CalendarMonth,
                    PeriodCode = "FY1920",
                    Year = 2020,
                    TypeValue = "March",
                    Occurence = 2,
                    ProfiledValue = periodValue
                }
            };
        }

        private class Org
        {
            public Type Type { get; set; }

            public string Name { get; set; }

            public string Code { get; set; }

            public string UKPRN { get; set; }

            public int RowNumber { get; set; }
        }

        private static List<Org> GetLasForRegion(string regionName)
        {
            switch (regionName)
            {
                case "London":
                case "LONDON":
                    return new List<Org>
                    {
                        //new Org { Code = "201", Name = "City of London", Type = Type.LA, UKPRN = "MOCKUKPRN201", RowNumber = 5 },
                        new Org { Code = "202", Name = "Camden", Type = Type.LA, UKPRN = "MOCKUKPRN202", RowNumber = 6 },
                        new Org { Code = "203", Name = "Greenwich", Type = Type.LA, UKPRN = "MOCKUKPRN203", RowNumber = 7 },
                        new Org { Code = "204", Name = "Hackney", Type = Type.LA, UKPRN = "MOCKUKPRN204", RowNumber = 8 },
                        new Org { Code = "205", Name = "Hammersmith and Fulham", Type = Type.LA, UKPRN = "MOCKUKPRN205", RowNumber = 9 },
                        new Org { Code = "206", Name = "Islington", Type = Type.LA, UKPRN = "MOCKUKPRN206", RowNumber = 10 },
                        new Org { Code = "207", Name = "Kensington and Chelsea", Type = Type.LA, UKPRN = "MOCKUKPRN207", RowNumber = 11 },
                        new Org { Code = "208", Name = "Lambeth", Type = Type.LA, UKPRN = "MOCKUKPRN208", RowNumber = 12 },
                        new Org { Code = "209", Name = "Lewisham", Type = Type.LA, UKPRN = "MOCKUKPRN209", RowNumber = 13 },
                        new Org { Code = "210", Name = "Southwark", Type = Type.LA, UKPRN = "MOCKUKPRN210", RowNumber = 14 },
                        new Org { Code = "211", Name = "Tower Hamlets", Type = Type.LA, UKPRN = "MOCKUKPRN211", RowNumber = 15 },
                        new Org { Code = "212", Name = "Wandsworth", Type = Type.LA, UKPRN = "MOCKUKPRN212", RowNumber = 16 },
                        new Org { Code = "213", Name = "Westminster", Type = Type.LA, UKPRN = "MOCKUKPRN213", RowNumber = 17 },
                        new Org { Code = "301", Name = "Barking and Dagenham", Type = Type.LA, UKPRN = "MOCKUKPRN301", RowNumber = 18 },
                        new Org { Code = "302", Name = "Barnet", Type = Type.LA, UKPRN = "MOCKUKPRN302", RowNumber = 19 },
                        new Org { Code = "303", Name = "Bexley", Type = Type.LA, UKPRN = "MOCKUKPRN303", RowNumber = 20 },
                        new Org { Code = "304", Name = "Brent", Type = Type.LA, UKPRN = "MOCKUKPRN304", RowNumber = 21 },
                        new Org { Code = "305", Name = "Bromley", Type = Type.LA, UKPRN = "MOCKUKPRN305", RowNumber = 22 },
                        new Org { Code = "306", Name = "Croydon", Type = Type.LA, UKPRN = "MOCKUKPRN306", RowNumber = 23 },
                        new Org { Code = "307", Name = "Ealing", Type = Type.LA, UKPRN = "MOCKUKPRN307", RowNumber = 24 },
                        new Org { Code = "308", Name = "Enfield", Type = Type.LA, UKPRN = "MOCKUKPRN308", RowNumber = 25 },
                        new Org { Code = "309", Name = "Haringey", Type = Type.LA, UKPRN = "MOCKUKPRN309", RowNumber = 26 },
                        new Org { Code = "310", Name = "Harrow", Type = Type.LA, UKPRN = "MOCKUKPRN310", RowNumber = 27 },
                        new Org { Code = "311", Name = "Havering", Type = Type.LA, UKPRN = "MOCKUKPRN311", RowNumber = 28 },
                        new Org { Code = "312", Name = "Hillingdon", Type = Type.LA, UKPRN = "MOCKUKPRN312", RowNumber = 29 },
                        new Org { Code = "313", Name = "Hounslow", Type = Type.LA, UKPRN = "MOCKUKPRN313", RowNumber = 30 },
                        new Org { Code = "314", Name = "Kingston upon Thames", Type = Type.LA, UKPRN = "MOCKUKPRN314", RowNumber = 31 },
                        new Org { Code = "315", Name = "Merton", Type = Type.LA, UKPRN = "MOCKUKPRN315", RowNumber = 32 },
                        new Org { Code = "316", Name = "Newham", Type = Type.LA, UKPRN = "MOCKUKPRN316", RowNumber = 33 },
                        new Org { Code = "317", Name = "Redbridge", Type = Type.LA, UKPRN = "MOCKUKPRN317", RowNumber = 34 },
                        new Org { Code = "318", Name = "Richmond upon Thames", Type = Type.LA, UKPRN = "MOCKUKPRN318", RowNumber = 35 },
                        new Org { Code = "319", Name = "Sutton", Type = Type.LA, UKPRN = "MOCKUKPRN319", RowNumber = 36 },
                        new Org { Code = "320", Name = "Waltham Forest", Type = Type.LA, UKPRN = "MOCKUKPRN320", RowNumber = 37 },
                    };
                case "METROPOLITAN AUTHORITIES":
                    return new List<Org>
                    {
                        new Org { Code = "330", Name = "Birmingham", Type = Type.LA, UKPRN = "MOCKUKPRN330", RowNumber = 37 },
                        new Org { Code = "331", Name = "Coventry", Type = Type.LA, UKPRN = "MOCKUKPRN331", RowNumber = 38 },
                        new Org { Code = "332", Name = "Dudley", Type = Type.LA, UKPRN = "MOCKUKPRN332", RowNumber = 39 },
                        new Org { Code = "333", Name = "Sandwell", Type = Type.LA, UKPRN = "MOCKUKPRN333", RowNumber = 40 },
                        new Org { Code = "334", Name = "Solihull", Type = Type.LA, UKPRN = "MOCKUKPRN334", RowNumber = 41 },
                        new Org { Code = "335", Name = "Walsall", Type = Type.LA, UKPRN = "MOCKUKPRN335", RowNumber = 42 },
                        new Org { Code = "336", Name = "Wolverhampton", Type = Type.LA, UKPRN = "MOCKUKPRN336", RowNumber = 43 },
                        new Org { Code = "340", Name = "Knowsley", Type = Type.LA, UKPRN = "MOCKUKPRN340", RowNumber = 44 },
                        new Org { Code = "341", Name = "Liverpool", Type = Type.LA, UKPRN = "MOCKUKPRN341", RowNumber = 45 },
                        new Org { Code = "342", Name = "St Helens", Type = Type.LA, UKPRN = "MOCKUKPRN342", RowNumber = 46 },
                        new Org { Code = "343", Name = "Sefton", Type = Type.LA, UKPRN = "MOCKUKPRN343", RowNumber = 47 },
                        new Org { Code = "344", Name = "Wirral", Type = Type.LA, UKPRN = "MOCKUKPRN344", RowNumber = 48 },
                        new Org { Code = "350", Name = "Bolton", Type = Type.LA, UKPRN = "MOCKUKPRN350", RowNumber = 49 },
                        new Org { Code = "351", Name = "Bury", Type = Type.LA, UKPRN = "MOCKUKPRN351", RowNumber = 50 },
                        new Org { Code = "352", Name = "Manchester", Type = Type.LA, UKPRN = "MOCKUKPRN352", RowNumber = 51 },
                        new Org { Code = "353", Name = "Oldham", Type = Type.LA, UKPRN = "MOCKUKPRN353", RowNumber = 52 },
                        new Org { Code = "354", Name = "Rochdale", Type = Type.LA, UKPRN = "MOCKUKPRN354", RowNumber = 53 },
                        new Org { Code = "355", Name = "Salford", Type = Type.LA, UKPRN = "MOCKUKPRN355", RowNumber = 54 },
                        new Org { Code = "356", Name = "Stockport", Type = Type.LA, UKPRN = "MOCKUKPRN356", RowNumber = 55 },
                        new Org { Code = "357", Name = "Tameside", Type = Type.LA, UKPRN = "MOCKUKPRN357", RowNumber = 56 },
                        new Org { Code = "358", Name = "Trafford", Type = Type.LA, UKPRN = "MOCKUKPRN358", RowNumber = 57 },
                        new Org { Code = "359", Name = "Wigan", Type = Type.LA, UKPRN = "MOCKUKPRN359", RowNumber = 58 }                        
                    };
                case "UNITARY AUTHORITIES":
                    return new List<Org>
                    {
                        new Org { Code = "800", Name = "Bath and North East Somerset", Type = Type.LA, UKPRN = "MOCKUKPRN800", RowNumber = 74 },
                        new Org { Code = "801", Name = "Bristol, City of", Type = Type.LA, UKPRN = "MOCKUKPRN801", RowNumber = 75 },
                        new Org { Code = "802", Name = "North Somerset", Type = Type.LA, UKPRN = "MOCKUKPRN802", RowNumber = 76 },
                        new Org { Code = "803", Name = "South Gloucestershire", Type = Type.LA, UKPRN = "MOCKUKPRN803", RowNumber = 77 },
                        new Org { Code = "805", Name = "Hartlepool", Type = Type.LA, UKPRN = "MOCKUKPRN805", RowNumber = 78 },
                        new Org { Code = "806", Name = "Middlesbrough", Type = Type.LA, UKPRN = "MOCKUKPRN806", RowNumber = 79 },
                        new Org { Code = "807", Name = "Redcar and Cleveland", Type = Type.LA, UKPRN = "MOCKUKPRN807", RowNumber = 80 },
                        new Org { Code = "808", Name = "Stockton-on-Tees", Type = Type.LA, UKPRN = "MOCKUKPRN808", RowNumber = 81 },
                        new Org { Code = "810", Name = "Kingston Upon Hull, City of", Type = Type.LA, UKPRN = "MOCKUKPRN810", RowNumber = 82 },
                        new Org { Code = "811", Name = "East Riding of Yorkshire", Type = Type.LA, UKPRN = "MOCKUKPRN811", RowNumber = 83 },
                        new Org { Code = "812", Name = "North East Lincolnshire", Type = Type.LA, UKPRN = "MOCKUKPRN812", RowNumber = 84 },
                        new Org { Code = "813", Name = "North Lincolnshire", Type = Type.LA, UKPRN = "MOCKUKPRN813", RowNumber = 85 },
                        new Org { Code = "816", Name = "York", Type = Type.LA, UKPRN = "MOCKUKPRN816", RowNumber = 87 },
                        new Org { Code = "821", Name = "Luton", Type = Type.LA, UKPRN = "MOCKUKPRN821", RowNumber = 88 },
                        new Org { Code = "822", Name = "Bedford Borough", Type = Type.LA, UKPRN = "MOCKUKPRN822", RowNumber = 89 },
                        new Org { Code = "823", Name = "Central Bedfordshire", Type = Type.LA, UKPRN = "MOCKUKPRN823", RowNumber = 90 },           
                        new Org { Code = "826", Name = "Milton Keynes", Type = Type.LA, UKPRN = "MOCKUKPRN826", RowNumber = 92 },
                        new Org { Code = "831", Name = "Derby", Type = Type.LA, UKPRN = "MOCKUKPRN831", RowNumber = 94 },
                        new Org { Code = "838", Name = "Dorset", Type = Type.LA, UKPRN = "MOCKUKPRN838", RowNumber = 95 },
                        new Org { Code = "839", Name = "Bournemouth, Poole and Dorset", Type = Type.LA, UKPRN = "MOCKUKPRN839", RowNumber = 96 },
                        new Org { Code = "840", Name = "Durham", Type = Type.LA, UKPRN = "MOCKUKPRN840", RowNumber = 97 },
                        new Org { Code = "841", Name = "Darlington", Type = Type.LA, UKPRN = "MOCKUKPRN841", RowNumber = 98 },
                        new Org { Code = "846", Name = "Brighton and Hove", Type = Type.LA, UKPRN = "MOCKUKPRN846", RowNumber = 100 },
                        new Org { Code = "851", Name = "Portsmouth", Type = Type.LA, UKPRN = "MOCKUKPRN851", RowNumber = 102 },
                        new Org { Code = "852", Name = "Southampton", Type = Type.LA, UKPRN = "MOCKUKPRN852", RowNumber = 103 },
                        new Org { Code = "856", Name = "Leicester", Type = Type.LA, UKPRN = "MOCKUKPRN856", RowNumber = 105 },
                        new Org { Code = "857", Name = "Rutland", Type = Type.LA, UKPRN = "MOCKUKPRN857", RowNumber = 106 },
                        new Org { Code = "861", Name = "Stoke-on-Trent", Type = Type.LA, UKPRN = "MOCKUKPRN861", RowNumber = 108 },
                        new Org { Code = "865", Name = "Wiltshire", Type = Type.LA, UKPRN = "MOCKUKPRN865", RowNumber = 109 },
                        new Org { Code = "866", Name = "Swindon", Type = Type.LA, UKPRN = "MOCKUKPRN866", RowNumber = 110 },
                        new Org { Code = "867", Name = "Bracknell Forest", Type = Type.LA, UKPRN = "MOCKUKPRN867", RowNumber = 111 },
                        new Org { Code = "868", Name = "Windsor and Maidenhead", Type = Type.LA, UKPRN = "MOCKUKPRN868", RowNumber = 112 },
                        new Org { Code = "869", Name = "West Berkshire", Type = Type.LA, UKPRN = "MOCKUKPRN869", RowNumber = 113 },
                        new Org { Code = "870", Name = "Reading", Type = Type.LA, UKPRN = "MOCKUKPRN870", RowNumber = 114 },
                        new Org { Code = "871", Name = "Slough", Type = Type.LA, UKPRN = "MOCKUKPRN871", RowNumber = 115 },
                        new Org { Code = "872", Name = "Wokingham", Type = Type.LA, UKPRN = "MOCKUKPRN872", RowNumber = 116 },
                        new Org { Code = "874", Name = "Peterborough", Type = Type.LA, UKPRN = "MOCKUKPRN874", RowNumber = 118 },
                        new Org { Code = "876", Name = "Halton", Type = Type.LA, UKPRN = "MOCKUKPRN876", RowNumber = 119 },
                        new Org { Code = "877", Name = "Warrington", Type = Type.LA, UKPRN = "MOCKUKPRN877", RowNumber = 120 },
                        new Org { Code = "879", Name = "Plymouth", Type = Type.LA, UKPRN = "MOCKUKPRN879", RowNumber = 122 },
                        new Org { Code = "880", Name = "Torbay", Type = Type.LA, UKPRN = "MOCKUKPRN880", RowNumber = 123 },
                        new Org { Code = "882", Name = "Southend-on-Sea", Type = Type.LA, UKPRN = "MOCKUKPRN882", RowNumber = 125 },
                        new Org { Code = "883", Name = "Thurrock", Type = Type.LA, UKPRN = "MOCKUKPRN883", RowNumber = 126 },
                        new Org { Code = "884", Name = "Herefordshire", Type = Type.LA, UKPRN = "MOCKUKPRN884", RowNumber = 127 },
                        new Org { Code = "887", Name = "Medway", Type = Type.LA, UKPRN = "MOCKUKPRN887", RowNumber = 130 },
                        new Org { Code = "889", Name = "Blackburn with Darwen", Type = Type.LA, UKPRN = "MOCKUKPRN889", RowNumber = 132 },
                        new Org { Code = "890", Name = "Blackpool", Type = Type.LA, UKPRN = "MOCKUKPRN890", RowNumber = 133 },
                        new Org { Code = "892", Name = "Nottingham", Type = Type.LA, UKPRN = "MOCKUKPRN892", RowNumber = 135 },
                        new Org { Code = "893", Name = "Shropshire", Type = Type.LA, UKPRN = "MOCKUKPRN893", RowNumber = 136 },
                        new Org { Code = "894", Name = "Telford and Wrekin", Type = Type.LA, UKPRN = "MOCKUKPRN894", RowNumber = 137 },
                        new Org { Code = "895", Name = "Cheshire East", Type = Type.LA, UKPRN = "MOCKUKPRN895", RowNumber = 138 },
                        new Org { Code = "896", Name = "Cheshire West and Chester", Type = Type.LA, UKPRN = "MOCKUKPRN896", RowNumber = 139 },
                        new Org { Code = "908", Name = "Cornwall", Type = Type.LA, UKPRN = "MOCKUKPRN908", RowNumber = 140 },
                        new Org { Code = "919", Name = "Hertfordshire", Type = Type.LA, UKPRN = "MOCKUKPRN919", RowNumber = 143 },
                        new Org { Code = "921", Name = "Isle of Wight", Type = Type.LA, UKPRN = "MOCKUKPRN921", RowNumber = 144 },
                        new Org { Code = "929", Name = "Northumberland", Type = Type.LA, UKPRN = "MOCKUKPRN929", RowNumber = 148 }
                    };
                case "UPPER TIER AUTHORITIES":
                    return new List<Org>
                    {
                        new Org { Code = "815", Name = "North Yorkshire", Type = Type.LA, UKPRN = "MOCKUKPRN815", RowNumber = 86 },
                        new Org { Code = "825", Name = "Buckinghamshire", Type = Type.LA, UKPRN = "MOCKUKPRN825", RowNumber = 91 },
                        new Org { Code = "830", Name = "Derbyshire", Type = Type.LA, UKPRN = "MOCKUKPRN830", RowNumber = 93 },
                        new Org { Code = "845", Name = "East Sussex", Type = Type.LA, UKPRN = "MOCKUKPRN845", RowNumber = 99 },
                        new Org { Code = "850", Name = "Hampshire", Type = Type.LA, UKPRN = "MOCKUKPRN850", RowNumber = 101 },
                        new Org { Code = "860", Name = "Staffordshire", Type = Type.LA, UKPRN = "MOCKUKPRN860", RowNumber = 107 },
                        new Org { Code = "873", Name = "Cambridgeshire", Type = Type.LA, UKPRN = "MOCKUKPRN873", RowNumber = 117 },
                        new Org { Code = "878", Name = "Devon", Type = Type.LA, UKPRN = "MOCKUKPRN878", RowNumber = 121 },
                        new Org { Code = "881", Name = "Essex", Type = Type.LA, UKPRN = "MOCKUKPRN881", RowNumber = 124 },
                        new Org { Code = "885", Name = "Worcestershire", Type = Type.LA, UKPRN = "MOCKUKPRN885", RowNumber = 128 },
                        new Org { Code = "888", Name = "Lancashire", Type = Type.LA, UKPRN = "MOCKUKPRN888", RowNumber = 131 },
                        new Org { Code = "891", Name = "Nottinghamshire", Type = Type.LA, UKPRN = "MOCKUKPRN891", RowNumber = 134 },
                        new Org { Code = "909", Name = "Cumbria", Type = Type.LA, UKPRN = "MOCKUKPRN909", RowNumber = 141 },
                        new Org { Code = "916", Name = "Gloucestershire", Type = Type.LA, UKPRN = "MOCKUKPRN916", RowNumber = 142 },
                        new Org { Code = "919", Name = "Hertfordshire", Type = Type.LA, UKPRN = "MOCKUKPRN919", RowNumber = 143 },
                        new Org { Code = "925", Name = "Lincolnshire", Type = Type.LA, UKPRN = "MOCKUKPRN925", RowNumber = 145 },
                        new Org { Code = "926", Name = "Norfolk", Type = Type.LA, UKPRN = "MOCKUKPRN926", RowNumber = 146 },
                        new Org { Code = "928", Name = "Northamptonshire", Type = Type.LA, UKPRN = "MOCKUKPRN928", RowNumber = 147 },
                        new Org { Code = "931", Name = "Oxfordshire", Type = Type.LA, UKPRN = "MOCKUKPRN931", RowNumber = 149 },
                        new Org { Code = "933", Name = "Somerset", Type = Type.LA, UKPRN = "MOCKUKPRN933", RowNumber = 150 },
                        new Org { Code = "935", Name = "Suffolk", Type = Type.LA, UKPRN = "MOCKUKPRN935", RowNumber = 151 },
                        new Org { Code = "936", Name = "Surrey", Type = Type.LA, UKPRN = "MOCKUKPRN936", RowNumber = 152 },
                        new Org { Code = "937", Name = "Warwickshire", Type = Type.LA, UKPRN = "MOCKUKPRN937", RowNumber = 153 },
                        new Org { Code = "938", Name = "West Sussex", Type = Type.LA, UKPRN = "MOCKUKPRN938", RowNumber = 154 }
                    };
                case "East of England":
                    return new List<Org>
                    {
                        new Org { Code = "821", Name = "Luton", Type = Type.LA, UKPRN = "MOCKUKPRN821", RowNumber = 88 },
                        new Org { Code = "822", Name = "Bedford Borough", Type = Type.LA, UKPRN = "MOCKUKPRN822", RowNumber = 89 },
                        new Org { Code = "823", Name = "Central Bedfordshire", Type = Type.LA, UKPRN = "MOCKUKPRN823", RowNumber = 90 },
                        new Org { Code = "873", Name = "Cambridgeshire", Type = Type.LA, UKPRN = "MOCKUKPRN873", RowNumber = 117 },
                        new Org { Code = "874", Name = "Peterborough", Type = Type.LA, UKPRN = "MOCKUKPRN874", RowNumber = 118 },
                        new Org { Code = "881", Name = "Essex", Type = Type.LA, UKPRN = "MOCKUKPRN881", RowNumber = 124 },
                        new Org { Code = "882", Name = "Southend-on-Sea", Type = Type.LA, UKPRN = "MOCKUKPRN882", RowNumber = 125 },
                        new Org { Code = "883", Name = "Thurrock", Type = Type.LA, UKPRN = "MOCKUKPRN883", RowNumber = 126 },
                        new Org { Code = "919", Name = "Hertfordshire", Type = Type.LA, UKPRN = "MOCKUKPRN919", RowNumber = 143 },
                        new Org { Code = "926", Name = "Norfolk", Type = Type.LA, UKPRN = "MOCKUKPRN926", RowNumber = 146 },
                        new Org { Code = "935", Name = "Suffolk", Type = Type.LA, UKPRN = "MOCKUKPRN935", RowNumber = 151 }
                    };
                case "East Midlands":
                    return new List<Org>
                    {
                        new Org { Code = "830", Name = "Derbyshire", Type = Type.LA, UKPRN = "MOCKUKPRN830", RowNumber = 93 },
                        new Org { Code = "831", Name = "Derby", Type = Type.LA, UKPRN = "MOCKUKPRN831", RowNumber = 94 },
                        new Org { Code = "855", Name = "Leicestershire", Type = Type.LA, UKPRN = "MOCKUKPRN855", RowNumber = 104 },
                        new Org { Code = "856", Name = "Leicester", Type = Type.LA, UKPRN = "MOCKUKPRN856", RowNumber = 105 },
                        new Org { Code = "857", Name = "Rutland", Type = Type.LA, UKPRN = "MOCKUKPRN857", RowNumber = 106 },
                        new Org { Code = "891", Name = "Nottinghamshire", Type = Type.LA, UKPRN = "MOCKUKPRN891", RowNumber = 134 },
                        new Org { Code = "892", Name = "Nottingham", Type = Type.LA, UKPRN = "MOCKUKPRN892", RowNumber = 135 },
                        new Org { Code = "925", Name = "Lincolnshire", Type = Type.LA, UKPRN = "MOCKUKPRN925", RowNumber = 145 },
                        new Org { Code = "928", Name = "Northamptonshire", Type = Type.LA, UKPRN = "MOCKUKPRN928", RowNumber = 147 }
                    };
                case "North East":
                    return new List<Org>
                    {
                        new Org { Code = "390", Name = "Gateshead", Type = Type.LA, UKPRN = "MOCKUKPRN390", RowNumber = 69 },
                        new Org { Code = "391", Name = "Newcastle upon Tyne", Type = Type.LA, UKPRN = "MOCKUKPRN391", RowNumber = 70 },
                        new Org { Code = "392", Name = "North Tyneside", Type = Type.LA, UKPRN = "MOCKUKPRN392", RowNumber = 71 },
                        new Org { Code = "393", Name = "South Tyneside", Type = Type.LA, UKPRN = "MOCKUKPRN393", RowNumber = 72 },
                        new Org { Code = "394", Name = "Sunderland", Type = Type.LA, UKPRN = "MOCKUKPRN394", RowNumber = 73 },
                        new Org { Code = "805", Name = "Hartlepool", Type = Type.LA, UKPRN = "MOCKUKPRN805", RowNumber = 78 },
                        new Org { Code = "806", Name = "Middlesbrough", Type = Type.LA, UKPRN = "MOCKUKPRN806", RowNumber = 79 },
                        new Org { Code = "807", Name = "Redcar and Cleveland", Type = Type.LA, UKPRN = "MOCKUKPRN807", RowNumber = 80 },
                        new Org { Code = "808", Name = "Stockton-on-Tees", Type = Type.LA, UKPRN = "MOCKUKPRN808", RowNumber = 81 },
                        new Org { Code = "840", Name = "Durham", Type = Type.LA, UKPRN = "MOCKUKPRN840", RowNumber = 97 },
                        new Org { Code = "841", Name = "Darlington", Type = Type.LA, UKPRN = "MOCKUKPRN841", RowNumber = 98 },
                        new Org { Code = "929", Name = "Northumberland", Type = Type.LA, UKPRN = "MOCKUKPRN929", RowNumber = 148 }
                    };
                case "North West":
                    return new List<Org>
                    {
                        new Org { Code = "340", Name = "Knowsley", Type = Type.LA, UKPRN = "MOCKUKPRN340", RowNumber = 45 },
                        new Org { Code = "341", Name = "Liverpool", Type = Type.LA, UKPRN = "MOCKUKPRN341", RowNumber = 46 },
                        new Org { Code = "342", Name = "St Helens", Type = Type.LA, UKPRN = "MOCKUKPRN342", RowNumber = 47 },
                        new Org { Code = "343", Name = "Sefton", Type = Type.LA, UKPRN = "MOCKUKPRN343", RowNumber = 48 },
                        new Org { Code = "344", Name = "Wirral", Type = Type.LA, UKPRN = "MOCKUKPRN344", RowNumber = 49 },
                        new Org { Code = "350", Name = "Bolton", Type = Type.LA, UKPRN = "MOCKUKPRN350", RowNumber = 50 },
                        new Org { Code = "351", Name = "Bury", Type = Type.LA, UKPRN = "MOCKUKPRN351", RowNumber = 51 },
                        new Org { Code = "352", Name = "Manchester", Type = Type.LA, UKPRN = "MOCKUKPRN352", RowNumber = 52 },
                        new Org { Code = "353", Name = "Oldham", Type = Type.LA, UKPRN = "MOCKUKPRN353", RowNumber = 53 },
                        new Org { Code = "354", Name = "Rochdale", Type = Type.LA, UKPRN = "MOCKUKPRN354", RowNumber = 54 },
                        new Org { Code = "355", Name = "Salford", Type = Type.LA, UKPRN = "MOCKUKPRN355", RowNumber = 55 },
                        new Org { Code = "356", Name = "Stockport", Type = Type.LA, UKPRN = "MOCKUKPRN356", RowNumber = 56 },
                        new Org { Code = "357", Name = "Tameside", Type = Type.LA, UKPRN = "MOCKUKPRN357", RowNumber = 57 },
                        new Org { Code = "358", Name = "Trafford", Type = Type.LA, UKPRN = "MOCKUKPRN358", RowNumber = 58 },
                        new Org { Code = "359", Name = "Wigan", Type = Type.LA, UKPRN = "MOCKUKPRN359", RowNumber = 59 },
                        new Org { Code = "876", Name = "Halton", Type = Type.LA, UKPRN = "MOCKUKPRN876", RowNumber = 119 },
                        new Org { Code = "877", Name = "Warrington", Type = Type.LA, UKPRN = "MOCKUKPRN877", RowNumber = 120 },
                        new Org { Code = "895", Name = "Cheshire East", Type = Type.LA, UKPRN = "MOCKUKPRN895", RowNumber = 138 },
                        new Org { Code = "896", Name = "Cheshire West and Chester", Type = Type.LA, UKPRN = "MOCKUKPRN896", RowNumber = 139 },
                        new Org { Code = "909", Name = "Cumbria", Type = Type.LA, UKPRN = "MOCKUKPRN909", RowNumber = 141 }
                    };
                case "South East":
                    return new List<Org>
                    {
                        new Org { Code = "825", Name = "Buckinghamshire", Type = Type.LA, UKPRN = "MOCKUKPRN825", RowNumber = 91 },
                        new Org { Code = "826", Name = "Milton Keynes", Type = Type.LA, UKPRN = "MOCKUKPRN826", RowNumber = 92 },
                        new Org { Code = "845", Name = "East Sussex", Type = Type.LA, UKPRN = "MOCKUKPRN845", RowNumber = 99 },
                        new Org { Code = "846", Name = "Brighton and Hove", Type = Type.LA, UKPRN = "MOCKUKPRN846", RowNumber = 100 },
                        new Org { Code = "850", Name = "Hampshire", Type = Type.LA, UKPRN = "MOCKUKPRN850", RowNumber = 101 },
                        new Org { Code = "851", Name = "Portsmouth", Type = Type.LA, UKPRN = "MOCKUKPRN851", RowNumber = 102 },
                        new Org { Code = "852", Name = "Southampton", Type = Type.LA, UKPRN = "MOCKUKPRN852", RowNumber = 103 },
                        new Org { Code = "867", Name = "Bracknell Forest", Type = Type.LA, UKPRN = "MOCKUKPRN867", RowNumber = 111 },
                        new Org { Code = "868", Name = "Windsor and Maidenhead", Type = Type.LA, UKPRN = "MOCKUKPRN868", RowNumber = 112 },
                        new Org { Code = "869", Name = "West Berkshire", Type = Type.LA, UKPRN = "MOCKUKPRN869", RowNumber = 113 },
                        new Org { Code = "870", Name = "Reading", Type = Type.LA, UKPRN = "MOCKUKPRN870", RowNumber = 114 },
                        new Org { Code = "871", Name = "Slough", Type = Type.LA, UKPRN = "MOCKUKPRN871", RowNumber = 115 },
                        new Org { Code = "886", Name = "Kent", Type = Type.LA, UKPRN = "MOCKUKPRN886", RowNumber = 129 },
                        new Org { Code = "887", Name = "Medway", Type = Type.LA, UKPRN = "MOCKUKPRN887", RowNumber = 130 },
                        new Org { Code = "921", Name = "Isle of Wight", Type = Type.LA, UKPRN = "MOCKUKPRN921", RowNumber = 144 },
                        new Org { Code = "931", Name = "Oxfordshire", Type = Type.LA, UKPRN = "MOCKUKPRN931", RowNumber = 149 },
                        new Org { Code = "936", Name = "Surrey", Type = Type.LA, UKPRN = "MOCKUKPRN936", RowNumber = 152 },
                        new Org { Code = "938", Name = "West Sussex", Type = Type.LA, UKPRN = "MOCKUKPRN938", RowNumber = 154 }
                    };
                case "South West":
                    return new List<Org>
                    {
                        new Org { Code = "800", Name = "Bath and North East Somerset", Type = Type.LA, UKPRN = "MOCKUKPRN800", RowNumber = 74 },
                        new Org { Code = "801", Name = "Bristol, City of", Type = Type.LA, UKPRN = "MOCKUKPRN801", RowNumber = 75 },
                        new Org { Code = "802", Name = "North Somerset", Type = Type.LA, UKPRN = "MOCKUKPRN802", RowNumber = 76 },
                        new Org { Code = "803", Name = "South Gloucestershire", Type = Type.LA, UKPRN = "MOCKUKPRN803", RowNumber = 77 },
                        new Org { Code = "838", Name = "Dorset", Type = Type.LA, UKPRN = "MOCKUKPRN838", RowNumber = 95 },
                        new Org { Code = "839", Name = "Bournemouth, Poole and Dorset", Type = Type.LA, UKPRN = "MOCKUKPRN839", RowNumber = 96 },
                        new Org { Code = "866", Name = "Swindon", Type = Type.LA, UKPRN = "MOCKUKPRN866", RowNumber = 110 },
                        new Org { Code = "867", Name = "Bracknell Forest", Type = Type.LA, UKPRN = "MOCKUKPRN867", RowNumber = 111 },
                        new Org { Code = "878", Name = "Devon", Type = Type.LA, UKPRN = "MOCKUKPRN878", RowNumber = 121 },
                        new Org { Code = "879", Name = "Plymouth", Type = Type.LA, UKPRN = "MOCKUKPRN879", RowNumber = 122 },
                        new Org { Code = "880", Name = "Torbay", Type = Type.LA, UKPRN = "MOCKUKPRN880", RowNumber = 123 },
                        new Org { Code = "908", Name = "Cornwall", Type = Type.LA, UKPRN = "MOCKUKPRN908", RowNumber = 140 },
                        new Org { Code = "916", Name = "Gloucestershire", Type = Type.LA, UKPRN = "MOCKUKPRN916", RowNumber = 142 }
                    };
                case "West Midlands":
                    return new List<Org>
                    {
                        new Org { Code = "330", Name = "Birmingham", Type = Type.LA, UKPRN = "MOCKUKPRN330", RowNumber = 38 },
                        new Org { Code = "331", Name = "Coventry", Type = Type.LA, UKPRN = "MOCKUKPRN331", RowNumber = 39 },
                        new Org { Code = "332", Name = "Dudley", Type = Type.LA, UKPRN = "MOCKUKPRN332", RowNumber = 40 },
                        new Org { Code = "333", Name = "Sandwell", Type = Type.LA, UKPRN = "MOCKUKPRN333", RowNumber = 41 },
                        new Org { Code = "334", Name = "Solihull", Type = Type.LA, UKPRN = "MOCKUKPRN334", RowNumber = 42 },
                        new Org { Code = "335", Name = "Walsall", Type = Type.LA, UKPRN = "MOCKUKPRN334", RowNumber = 43 },
                        new Org { Code = "336", Name = "Wolverhampton", Type = Type.LA, UKPRN = "MOCKUKPRN335", RowNumber = 44 },
                        new Org { Code = "860", Name = "Staffordshire", Type = Type.LA, UKPRN = "MOCKUKPRN860", RowNumber = 107 },
                        new Org { Code = "861", Name = "Stoke-on-Trent", Type = Type.LA, UKPRN = "MOCKUKPRN861", RowNumber = 108 },
                        new Org { Code = "884", Name = "Herefordshire", Type = Type.LA, UKPRN = "MOCKUKPRN884", RowNumber = 127 },
                        new Org { Code = "885", Name = "Worcestershire", Type = Type.LA, UKPRN = "MOCKUKPRN885", RowNumber = 128 },
                        new Org { Code = "893", Name = "Shropshire", Type = Type.LA, UKPRN = "MOCKUKPRN893", RowNumber = 136 },
                        new Org { Code = "894", Name = "Telford and Wrekin", Type = Type.LA, UKPRN = "MOCKUKPRN894", RowNumber = 137 },
                        new Org { Code = "937", Name = "Warwickshire", Type = Type.LA, UKPRN = "MOCKUKPRN937", RowNumber = 153 }
                    };
                case "Yorkshire and the Humber":
                    return new List<Org>
                    {
                        new Org { Code = "370", Name = "Barnsley", Type = Type.LA, UKPRN = "MOCKUKPRN370", RowNumber = 60 },
                        new Org { Code = "371", Name = "Doncaster", Type = Type.LA, UKPRN = "MOCKUKPRN371", RowNumber = 61 },
                        new Org { Code = "372", Name = "Rotherham", Type = Type.LA, UKPRN = "MOCKUKPRN372", RowNumber = 62 },
                        new Org { Code = "373", Name = "Sheffield", Type = Type.LA, UKPRN = "MOCKUKPRN373", RowNumber = 63 },
                        new Org { Code = "380", Name = "Bradford", Type = Type.LA, UKPRN = "MOCKUKPRN380", RowNumber = 64 },
                        new Org { Code = "381", Name = "Calderdale", Type = Type.LA, UKPRN = "MOCKUKPRN381", RowNumber = 65 },
                        new Org { Code = "382", Name = "Kirklees", Type = Type.LA, UKPRN = "MOCKUKPRN382", RowNumber = 66 },
                        new Org { Code = "383", Name = "Leeds", Type = Type.LA, UKPRN = "MOCKUKPRN383", RowNumber = 67 },
                        new Org { Code = "384", Name = "Wakefield", Type = Type.LA, UKPRN = "MOCKUKPRN384", RowNumber = 68 },
                        new Org { Code = "810", Name = "Kingston Upon Hull, City of", Type = Type.LA, UKPRN = "MOCKUKPRN810", RowNumber = 82 },
                        new Org { Code = "811", Name = "East Riding of Yorkshire", Type = Type.LA, UKPRN = "MOCKUKPRN811", RowNumber = 83 },
                        new Org { Code = "812", Name = "North East Lincolnshire", Type = Type.LA, UKPRN = "MOCKUKPRN812", RowNumber = 84 },
                        new Org { Code = "813", Name = "North Lincolnshire", Type = Type.LA, UKPRN = "MOCKUKPRN813", RowNumber = 85 },
                        new Org { Code = "815", Name = "North Yorkshire", Type = Type.LA, UKPRN = "MOCKUKPRN815", RowNumber = 86 },
                        new Org { Code = "816", Name = "York", Type = Type.LA, UKPRN = "MOCKUKPRN816", RowNumber = 87 }
                    };
            }

            throw new Exception("Can't find child LAs");
        }

        private enum Type
        {
            LA,
            Region,
            LocalGovernmentGroup
        }
    }
}
 