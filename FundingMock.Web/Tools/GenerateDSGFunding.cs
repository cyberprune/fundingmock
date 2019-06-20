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
        private static string GetDataString(DataSet spreadsheet, int sheetNumber, int rowIdx, int columnIdx)
        {
            var val = spreadsheet.Tables[sheetNumber].Rows[rowIdx]["Column" + columnIdx];

            switch (val.GetType().Name)
            {
                case "String":
                    return (string)val;
                case "Double":
                    return ((double)val).ToString();
            }

            return "--";
        }

        private static long GetDataFromMillions(DataSet spreadsheet, int sheetNumber, int rowIdx, int columnIdx)
        {
            var val = spreadsheet.Tables[sheetNumber].Rows[rowIdx]["Column" + columnIdx];

            switch (val.GetType().Name)
            {
                case "Double":
                    var transposed = (double)val * 1e8; // 1e8 rather then 1e6 as want in millions of pence, not pounds
                    return Convert.ToInt64(transposed);
            }

            throw new Exception("Can't convert from millions");
        }

        private static long GetDataInPence(DataSet spreadsheet, int sheetNumber, int rowIdx, int columnIdx)
        {
            var val = spreadsheet.Tables[sheetNumber].Rows[rowIdx]["Column" + columnIdx];

            switch (val.GetType().Name)
            {
                case "Double":
                    var transposed = (double)val * 1e2;
                    return Convert.ToInt64(transposed);
            }

            throw new Exception("Can't convert to pence");
        }

        private static long GetData(DataSet spreadsheet, int sheetNumber, int rowIdx, int columnIdx)
        {
            var val = spreadsheet.Tables[sheetNumber].Rows[rowIdx]["Column" + columnIdx];

            switch (val.GetType().Name)
            {
                case "Double":
                    return Convert.ToInt64((double)val);
            }

            throw new Exception("Can't read as double");
        }

        public static LogicalBaseModel[] Generate()
        {
            var schemaUri = "http://example.org/#schema";
            var schemaVersion = "1.0";
            var ukOffset = new TimeSpan(0, 0, 0);

            var las = new List<LA>();
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

            for (var idx = 5; idx < 154; idx++)
            {
                las.Add(new LA
                {
                    Code = GetDataString(spreadsheet, 1, 5, 0),
                    Name = GetDataString(spreadsheet, 1, 5, 1),
                    UKPRN = "EXAMPLE",
                    RowNumber = idx
                });
            }

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

            var returnList = new List<LogicalBaseModel>();

            foreach (var la in las)
            {
                var identifiers = new List<OrganisationIdentifier>
                {
                    new OrganisationIdentifier
                    {
                        Type = Enums.OrganisationIdentifierType.LACode,
                        Value = la.Code
                    },
                    new OrganisationIdentifier
                    {
                        Type = Enums.OrganisationIdentifierType.UKPRN,
                        Value = la.UKPRN
                    }
                };

                var groupingOrg = new OrganisationGroup
                {
                    Name = la.Name,
                    Type = Enums.OrganisationType.LocalAuthority,
                    SearchableName = la.Name,
                    Identifiers = identifiers
                };

                var fundingVersion = "1.0";

                uint templateLineId = 1;
                uint templateCalculationId = 1;
                uint templateReferenceId = 1;

                var periodValue = Convert.ToInt64(GetDataFromMillions(spreadsheet, 1, la.RowNumber, 11) / 25.0);
                var periods = new List<FundingLinePeriod>
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

                var fundingValue = new FundingValue
                {
                    TotalValue = GetDataFromMillions(spreadsheet, 1, la.RowNumber, 11),
                    FundingValueByDistributionPeriod = new List<FundingValueByDistributionPeriod>
                    {
                        new FundingValueByDistributionPeriod
                        {
                            DistributionPeriodCode = "FY1920",
                            Value = GetDataFromMillions(spreadsheet, 1, la.RowNumber, 11),
                            FundingLines = new List<FundingLine>
                            {
                                new FundingLine
                                {
                                    Name = "PriorToRecoupment",
                                    TemplateLineId = templateLineId++,
                                    Type = FundingLineType.Information,
                                    Value = GetDataFromMillions(spreadsheet, 1, la.RowNumber, 7),
                                    FundingLines = new List<FundingLine>
                                    {
                                        new FundingLine
                                        {
                                            Name = "School Block",
                                            TemplateLineId = templateLineId++,
                                            Type = FundingLineType.Information,
                                            Value = GetDataFromMillions(spreadsheet, 1, la.RowNumber, 3),
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
                                                            Value = GetDataFromMillions(spreadsheet, 2, la.RowNumber, 7),
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
                                                                    Value = GetDataFromMillions(spreadsheet, 2, la.RowNumber, 7),
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
                                                            Value = GetDataFromMillions(spreadsheet, 2, la.RowNumber, 8),
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
                                                                    Value = GetDataFromMillions(spreadsheet, 2, la.RowNumber, 8)
                                                                }
                                                            }
                                                        }
                                                    }
                                                },
                                                new FundingLine
                                                {
                                                    Name = "Pupil Led funding",
                                                    TemplateLineId = templateLineId++,
                                                    Value = (GetData(spreadsheet, 2, la.RowNumber, 4) * GetDataInPence(spreadsheet, 2, la.RowNumber, 2)) +
                                                        GetData(spreadsheet, 2, la.RowNumber, 5) * GetDataInPence(spreadsheet, 2, la.RowNumber, 3),
                                                    Calculations = new List<Calculation>
                                                    {
                                                        new Calculation
                                                        {
                                                            Name = "Primary Pupil funding",
                                                            Type = Enums.CalculationType.PerPupilFunding,
                                                            Value = GetData(spreadsheet, 2, la.RowNumber, 4) * GetDataInPence(spreadsheet, 2, la.RowNumber, 2),
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
                                                                    Value = GetData(spreadsheet, 2, la.RowNumber, 4)
                                                                },
                                                                new ReferenceData
                                                                {
                                                                    Name = "2019-20 schools block primary unit of funding",
                                                                    Format = Enums.ReferenceDataValueFormat.Number,
                                                                    TemplateReferenceId = templateReferenceId++,
                                                                    Value = GetDataInPence(spreadsheet, 2, la.RowNumber, 2)
                                                                }
                                                            }
                                                        },
                                                        new Calculation
                                                        {
                                                            Name = "Secondary Pupil funding",
                                                            Type = Enums.CalculationType.PerPupilFunding,
                                                            Value = GetData(spreadsheet, 2, la.RowNumber, 5) * GetDataInPence(spreadsheet, 2, la.RowNumber, 3),
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
                                                                    Value = GetData(spreadsheet, 2, la.RowNumber, 5)
                                                                },
                                                                new ReferenceData
                                                                {
                                                                    Name = "2019-20 schools block secondary unit of funding",
                                                                    Format = Enums.ReferenceDataValueFormat.Number,
                                                                    TemplateReferenceId = templateReferenceId++,
                                                                    Value = GetDataInPence(spreadsheet, 2, la.RowNumber, 3)
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
                                            Value = GetDataFromMillions(spreadsheet, 1, la.RowNumber, 4),
                                            FundingLines = new List<FundingLine>
                                            {
                                                new FundingLine
                                                {
                                                    Name = "CSSB Pupil Led Funding",
                                                    Value = GetData(spreadsheet, 2, la.RowNumber, 12) * GetDataInPence(spreadsheet, 2, la.RowNumber, 11),
                                                    TemplateLineId = templateLineId++,
                                                    Calculations = new List<Calculation>
                                                    {
                                                        new Calculation
                                                        {
                                                            Name = "CSSB Pupil funding",
                                                            Type = Enums.CalculationType.PerPupilFunding,
                                                            Value = GetData(spreadsheet, 2, la.RowNumber, 12) * GetDataInPence(spreadsheet, 2, la.RowNumber, 11),
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
                                                                    Value = GetData(spreadsheet, 2, la.RowNumber, 12)
                                                                },
                                                                new ReferenceData
                                                                {
                                                                    Name = "2019-20 CSSB unit of funding (£s)",
                                                                    Format = Enums.ReferenceDataValueFormat.Number,
                                                                    TemplateReferenceId = templateReferenceId++,
                                                                    Value = GetDataInPence(spreadsheet, 2, la.RowNumber, 11)
                                                                }
                                                            }
                                                        }
                                                    }
                                                },
                                                new FundingLine
                                                {
                                                    Name = "Funding for historic commitments",
                                                    TemplateLineId = templateLineId++,
                                                    Value = GetDataFromMillions(spreadsheet, 2, la.RowNumber, 13),
                                                    Calculations = new List<Calculation>
                                                    {
                                                        new Calculation
                                                        {
                                                            Name = "2019-20 CSSB funding for historic commitments",
                                                            Type = Enums.CalculationType.LumpSum,
                                                            Value = GetDataFromMillions(spreadsheet, 2, la.RowNumber, 13),
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
                                                                    Value = GetDataFromMillions(spreadsheet, 2, la.RowNumber, 13)
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
                                            Value = GetDataFromMillions(spreadsheet, 1, la.RowNumber, 5),
                                            FundingLines = new List<FundingLine>
                                            {
                                                new FundingLine
                                                {
                                                    Name = "Actual high needs fundings, excluding basic entitelment factor and import/export adjustments",
                                                    TemplateLineId = templateLineId++,
                                                    Value = GetDataFromMillions(spreadsheet, 3, la.RowNumber, 2),
                                                    Calculations = new List<Calculation>
                                                    {
                                                        new Calculation
                                                        {
                                                            Name = "HN Block Before Basic Entitlement and Import and Export Adjustments",
                                                            Type = Enums.CalculationType.LumpSum,
                                                            Value = GetDataFromMillions(spreadsheet, 3, la.RowNumber, 2),
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
                                                                    Value = GetDataFromMillions(spreadsheet, 3, la.RowNumber, 2)
                                                                }
                                                            }
                                                        }
                                                    }
                                                },
                                                new FundingLine
                                                {
                                                    Name = "Basic entitlements",
                                                    TemplateLineId = templateLineId++,
                                                    Value = GetDataInPence(spreadsheet, 3, la.RowNumber, 3) * GetData(spreadsheet, 3, la.RowNumber, 4),
                                                    Calculations = new List<Calculation>
                                                    {
                                                        new Calculation
                                                        {
                                                            Name = "HN Block Before Basic Entitlement and Import and Export Adjustments",
                                                            Type = Enums.CalculationType.LumpSum,
                                                            Value = GetDataInPence(spreadsheet, 3, la.RowNumber, 3) * GetData(spreadsheet, 3, la.RowNumber, 4),
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
                                                                    Value = GetData(spreadsheet, 3, la.RowNumber, 4)
                                                                },
                                                                new ReferenceData
                                                                {
                                                                    Name = "2019-20 ACA-weighted basic entitlement factor unit rate",
                                                                    Format = Enums.ReferenceDataValueFormat.Number,
                                                                    TemplateReferenceId = templateReferenceId++,
                                                                    Value = GetDataInPence(spreadsheet, 3, la.RowNumber, 3)
                                                                }
                                                            }
                                                        }
                                                    }
                                                },
                                                new FundingLine
                                                {
                                                    Name = "Import / export adjustments",
                                                    TemplateLineId = templateLineId++,
                                                    Value = GetDataFromMillions(spreadsheet, 3, la.RowNumber, 5),
                                                    Calculations = new List<Calculation>
                                                    {
                                                        new Calculation
                                                        {
                                                            Name = "Import/Export Adjustment",
                                                            Type = Enums.CalculationType.LumpSum,
                                                            Value = GetDataFromMillions(spreadsheet, 3, la.RowNumber, 5),
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
                                                                    Value = GetDataFromMillions(spreadsheet, 3, la.RowNumber, 5)
                                                                }
                                                            }
                                                        }
                                                    }
                                                },
                                                new FundingLine
                                                {
                                                    Name = "ONS Population Projection",
                                                    TemplateLineId = templateLineId++,
                                                    Value = GetData(spreadsheet, 3, la.RowNumber, 6),
                                                    Calculations = new List<Calculation>
                                                    {
                                                        new Calculation
                                                        {
                                                            Name = "ONS Population Projection",
                                                            Type = Enums.CalculationType.PupilNumber,
                                                            Value = GetData(spreadsheet, 3, la.RowNumber, 6),
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
                                                                    Value = GetData(spreadsheet, 3, la.RowNumber, 6)
                                                                }
                                                            }
                                                        }
                                                    }
                                                },
                                                new FundingLine
                                                {
                                                    Name = "Additional High Needs Funding",
                                                    TemplateLineId = templateLineId++,
                                                    Value = GetDataFromMillions(spreadsheet, 3, la.RowNumber, 7),
                                                    Calculations = new List<Calculation>
                                                    {
                                                        new Calculation
                                                        {
                                                            Name = "Additional High Needs Funding",
                                                            Type = Enums.CalculationType.LumpSum,
                                                            Value = GetDataFromMillions(spreadsheet, 3, la.RowNumber, 7),
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
                                                                    Value = GetDataFromMillions(spreadsheet, 3, la.RowNumber, 7)
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
                                            Value = GetDataFromMillions(spreadsheet, 1, la.RowNumber, 6),
                                            FundingLines = new List<FundingLine>
                                            {
                                                new FundingLine
                                                {
                                                    Name = "Initial funding funding for universal entitlement for 3 and 4 year olds",
                                                    TemplateLineId = templateLineId++,
                                                    Value = GetData(spreadsheet, 5, la.RowNumber, 3) *  GetDataInPence(spreadsheet, 5, la.RowNumber, 2) * 15 * 38,
                                                    Calculations = new List<Calculation>
                                                    {
                                                        new Calculation
                                                        {
                                                            Name = "HN Block Before Basic Entitlement and Import and Export Adjustments",
                                                            Type = Enums.CalculationType.PerPupilFunding,
                                                            Value = GetData(spreadsheet, 5, la.RowNumber, 3) *  GetDataInPence(spreadsheet, 5, la.RowNumber, 2) * 15 * 38,
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
                                                                    Value = GetData(spreadsheet, 5, la.RowNumber, 3)
                                                                },
                                                                new ReferenceData
                                                                {
                                                                    Name = "Early Years Universal Entitlement for 3 and 4 Year Olds Rate",
                                                                    Format = Enums.ReferenceDataValueFormat.Number,
                                                                    TemplateReferenceId = templateReferenceId++,
                                                                    Value = GetDataInPence(spreadsheet, 5, la.RowNumber, 2)
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
                                                    Value = GetData(spreadsheet, 5, la.RowNumber, 5) *  GetDataInPence(spreadsheet, 5, la.RowNumber, 2) * 15 * 38,
                                                    Calculations = new List<Calculation>
                                                    {
                                                        new Calculation
                                                        {
                                                            Name = "HN Block Before Basic Entitlement and Import and Export Adjustment",
                                                            Type = Enums.CalculationType.PerPupilFunding,
                                                            Value = GetData(spreadsheet, 5, la.RowNumber, 5) *  GetDataInPence(spreadsheet, 5, la.RowNumber, 2) * 15 * 38,
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
                                                                    Value = GetData(spreadsheet, 5, la.RowNumber, 5)
                                                                },
                                                                new ReferenceData
                                                                {
                                                                    Name = "Early Years Universal Entitlement for 3 and 4 Year Olds Rate",
                                                                    Format = Enums.ReferenceDataValueFormat.Number,
                                                                    TemplateReferenceId = templateReferenceId++,
                                                                    Value = GetDataInPence(spreadsheet, 5, la.RowNumber, 2)
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
                                                    Value = GetData(spreadsheet, 5, la.RowNumber, 8) *  GetDataInPence(spreadsheet, 5, la.RowNumber, 7) * 15 * 38,
                                                    Calculations = new List<Calculation>
                                                    {
                                                        new Calculation
                                                        {
                                                            Name = "Per Pupil Funding for 2 year old entitlement",
                                                            Type = Enums.CalculationType.PerPupilFunding,
                                                            Value = GetData(spreadsheet, 5, la.RowNumber, 8) *  GetDataInPence(spreadsheet, 5, la.RowNumber, 7) * 15 * 38,
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
                                                                    Value = GetData(spreadsheet, 5, la.RowNumber, 8)
                                                                },
                                                                new ReferenceData
                                                                {
                                                                    Name = "LA hourly rate for 2 year old entitlement",
                                                                    Format = Enums.ReferenceDataValueFormat.Number,
                                                                    TemplateReferenceId = templateReferenceId++,
                                                                    Value = GetDataInPence(spreadsheet, 5, la.RowNumber, 7)
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
                                                    Value = GetDataFromMillions(spreadsheet, 2, la.RowNumber, 10),
                                                    Calculations = new List<Calculation>
                                                    {
                                                        new Calculation
                                                        {
                                                            Name = "Early Years Pupil Premium lumpsum",
                                                            Type = Enums.CalculationType.LumpSum,
                                                            Value = GetDataFromMillions(spreadsheet, 2, la.RowNumber, 10),
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
                                                    Value = GetDataFromMillions(spreadsheet, 2, la.RowNumber, 11),
                                                    Calculations = new List<Calculation>
                                                    {
                                                        new Calculation
                                                        {
                                                            Name = "Disability Access Fund lumpsum",
                                                            Type = Enums.CalculationType.LumpSum,
                                                            Value = GetDataFromMillions(spreadsheet, 2, la.RowNumber, 11),
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
                                                    Value = GetDataFromMillions(spreadsheet, 2, la.RowNumber, 12),
                                                    Calculations = new List<Calculation>
                                                    {
                                                        new Calculation
                                                        {
                                                            Name = "Per Pupil Funding for Maintained Nursery Schools",
                                                            Type = Enums.CalculationType.PerPupilFunding,
                                                            Value = GetDataFromMillions(spreadsheet, 2, la.RowNumber, 12),
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
                                    TemplateLineId = templateLineId++,
                                    Type = FundingLineType.Payment,
                                    Value = GetDataFromMillions(spreadsheet, 1, la.RowNumber, 11),
                                    ProfilePeriods = periods,
                                    FundingLines = new List<FundingLine>
                                    {
                                        new FundingLine
                                        {
                                            Name = "School Block",
                                            TemplateLineId = templateLineId++,
                                            Type = FundingLineType.Information,
                                            Value = GetDataFromMillions(spreadsheet, 1, la.RowNumber, 7),
                                            FundingLines = new List<FundingLine>
                                            {
                                                new FundingLine
                                                {
                                                    Name = "Funding through the mobility and premises factors",
                                                    TemplateLineId = templateLineId++,
                                                    Value = GetDataFromMillions(spreadsheet, 2, la.RowNumber, 6),
                                                    Calculations = new List<Calculation>
                                                    {
                                                        new Calculation
                                                        {
                                                            Name = "2019-20 mobility and premises funding",
                                                            Type = Enums.CalculationType.LumpSum,
                                                            Value = GetDataFromMillions(spreadsheet, 2, la.RowNumber, 6),
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
                                                                    Value = GetDataFromMillions(spreadsheet, 2, la.RowNumber, 6)
                                                                }
                                                            }
                                                        }
                                                    }
                                                },
                                                new FundingLine
                                                {
                                                    Name = "Growth funding",
                                                    TemplateLineId = templateLineId++,
                                                    Value = GetDataFromMillions(spreadsheet, 2, la.RowNumber, 7),
                                                    Calculations = new List<Calculation>
                                                    {
                                                        new Calculation
                                                        {
                                                            Name = "2019-20 growth funding",
                                                            Type = Enums.CalculationType.LumpSum,
                                                            Value = GetDataFromMillions(spreadsheet, 2, la.RowNumber, 7),
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
                                                                    Value = GetDataFromMillions(spreadsheet, 2, la.RowNumber, 7)
                                                                }
                                                            }
                                                        }
                                                    }
                                                },
                                                new FundingLine
                                                {
                                                    Name = "Pupil Led funding",
                                                    TemplateLineId = templateLineId++,
                                                    Value = GetDataFromMillions(spreadsheet, 2, la.RowNumber, 2) * GetData(spreadsheet, 2, la.RowNumber, 4),
                                                    Calculations = new List<Calculation>
                                                    {
                                                        new Calculation
                                                        {
                                                            Name = "2019-20 schools block primary unit of funding",
                                                            Type = Enums.CalculationType.PerPupilFunding,
                                                            Value = GetDataFromMillions(spreadsheet, 2, la.RowNumber, 2) * GetData(spreadsheet, 2, la.RowNumber, 4),
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
                                                                    Value = GetData(spreadsheet, 2, la.RowNumber, 4)
                                                                },
                                                                new ReferenceData
                                                                {
                                                                    Name = "2019-20 schools block primary unit of funding",
                                                                    Format = Enums.ReferenceDataValueFormat.Number,
                                                                    TemplateReferenceId = templateReferenceId++,
                                                                    Value = GetDataFromMillions(spreadsheet, 2, la.RowNumber, 2)
                                                                }
                                                            }
                                                        }
                                                    }
                                                },
                                                new FundingLine
                                                {
                                                    Name = "Deductions To School Block",
                                                    TemplateLineId = templateLineId++,
                                                    Value = GetDataFromMillions(spreadsheet, 2, la.RowNumber, 9),
                                                    Calculations = new List<Calculation>
                                                    {
                                                        new Calculation
                                                        {
                                                            Name = "Deductions To School Block",
                                                            Type = Enums.CalculationType.LumpSum,
                                                            Value =  GetDataFromMillions(spreadsheet, 2, la.RowNumber, 9),
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
                                            Value = GetDataFromMillions(spreadsheet, 1, la.RowNumber, 8),
                                            FundingLines = new List<FundingLine>
                                            {
                                                new FundingLine
                                                {
                                                    Name = "CSSB Pupil Led Funding",
                                                    TemplateLineId = templateLineId++,
                                                    Value = GetDataInPence(spreadsheet, 2, la.RowNumber, 11) * GetData(spreadsheet, 2, la.RowNumber, 12),
                                                    Calculations = new List<Calculation>
                                                    {
                                                        new Calculation
                                                        {
                                                            Name = "Rate Deductions To School Block",
                                                            Type = Enums.CalculationType.PerPupilFunding,
                                                            Value = GetDataInPence(spreadsheet, 2, la.RowNumber, 11) * GetData(spreadsheet, 2, la.RowNumber, 12),
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
                                                                    Value = GetData(spreadsheet, 2, la.RowNumber, 12)
                                                                },
                                                                new ReferenceData
                                                                {
                                                                    Name = "2019-20 CSSB unit of funding (£s)",
                                                                    Format = Enums.ReferenceDataValueFormat.Number,
                                                                    TemplateReferenceId = templateReferenceId++,
                                                                    Value = GetDataInPence(spreadsheet, 2, la.RowNumber, 11)
                                                                }
                                                            }
                                                        }
                                                    }
                                                },
                                                new FundingLine
                                                {
                                                    Name = "Funding for historic commitments",
                                                    Value = GetDataFromMillions(spreadsheet, 2, la.RowNumber, 13),
                                                    Calculations = new List<Calculation>
                                                    {
                                                        new Calculation
                                                        {
                                                            Name = "2019-20 CSSB funding for historic commitments",
                                                            Type = Enums.CalculationType.LumpSum,
                                                            Value = GetDataFromMillions(spreadsheet, 2, la.RowNumber, 13),
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
                                                                    Value = GetDataFromMillions(spreadsheet, 2, la.RowNumber, 13)
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
                                            Value = GetDataFromMillions(spreadsheet, 1, la.RowNumber, 9),
                                            FundingLines = new List<FundingLine>
                                            {
                                                new FundingLine
                                                {
                                                    Name = "Actual high needs fundings, excluding basic entitelment factor and import/export adjustments",
                                                    Value = GetDataFromMillions(spreadsheet, 3, la.RowNumber, 2),
                                                    Calculations = new List<Calculation>
                                                    {
                                                        new Calculation
                                                        {
                                                            Name = "HN Block Before Basic Entitlement and Import and Export Adjustments",
                                                            Type = Enums.CalculationType.LumpSum,
                                                            Value = GetDataFromMillions(spreadsheet, 3, la.RowNumber, 2),
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
                                                                    Value = GetDataFromMillions(spreadsheet, 3, la.RowNumber, 2)
                                                                }
                                                            }
                                                        }
                                                    }
                                                },
                                                new FundingLine
                                                {
                                                    Name = "Basic entitlements",
                                                    Value = GetData(spreadsheet, 3, la.RowNumber, 4) * GetDataInPence(spreadsheet, 3, la.RowNumber, 3),
                                                    Calculations = new List<Calculation>
                                                    {
                                                        new Calculation
                                                        {
                                                            Name = "HN Block Before Basic Entitlement and Import and Export Adjustments",
                                                            Type = Enums.CalculationType.PerPupilFunding,
                                                            Value = GetData(spreadsheet, 3, la.RowNumber, 4) * GetDataInPence(spreadsheet, 3, la.RowNumber, 3),
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
                                                                    Value = GetData(spreadsheet, 3, la.RowNumber, 4)
                                                                },
                                                                new ReferenceData
                                                                {
                                                                    Name = "2019-20 ACA-weighted basic entitlement factor unit rate",
                                                                    Format = Enums.ReferenceDataValueFormat.Number,
                                                                    TemplateReferenceId = templateReferenceId++,
                                                                    Value = GetDataInPence(spreadsheet, 3, la.RowNumber, 3)
                                                                }
                                                            }
                                                        }
                                                    }
                                                },
                                                new FundingLine
                                                {
                                                    Name = "Import / export adjustments",
                                                    Value = GetDataFromMillions(spreadsheet, 3, la.RowNumber, 5),
                                                    Calculations = new List<Calculation>
                                                    {
                                                        new Calculation
                                                        {
                                                            Name = "Import/Export Adjustment",
                                                            Type = Enums.CalculationType.LumpSum,
                                                            Value = GetDataFromMillions(spreadsheet, 3, la.RowNumber, 5),
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
                                                    Value = GetData(spreadsheet, 3, la.RowNumber, 6),
                                                    Calculations = new List<Calculation>
                                                    {
                                                        new Calculation
                                                        {
                                                            Name = "ONS Population Projection",
                                                            Type = Enums.CalculationType.PupilNumber,
                                                            Value = GetData(spreadsheet, 3, la.RowNumber, 6),
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
                                                                    Value = GetData(spreadsheet, 3, la.RowNumber, 6),
                                                                }
                                                            }
                                                        }
                                                    }
                                                },
                                                new FundingLine
                                                {
                                                    Name = "Additional High Needs Funding",
                                                    Value = GetDataFromMillions(spreadsheet, 3, la.RowNumber, 7),
                                                    Calculations = new List<Calculation>
                                                    {
                                                        new Calculation
                                                        {
                                                            Name = "Additional High Needs Funding",
                                                            Type = Enums.CalculationType.LumpSum,
                                                            Value = GetDataFromMillions(spreadsheet, 3, la.RowNumber, 7),
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
                                                                    Value = GetDataFromMillions(spreadsheet, 3, la.RowNumber, 7)
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
                                                            Value = GetDataFromMillions(spreadsheet, 4, la.RowNumber, 4) + GetDataFromMillions(spreadsheet, 4, la.RowNumber, 7)
                                                                + GetDataFromMillions(spreadsheet, 4, la.RowNumber, 10) + GetDataFromMillions(spreadsheet, 4, la.RowNumber, 13),
                                                            FundingLines = new List<FundingLine>
                                                            {
                                                                new FundingLine
                                                                {
                                                                    Name = "Pre-16 SEN Places @ £6k",
                                                                    Value = GetDataFromMillions(spreadsheet, 4, la.RowNumber, 4),
                                                                    Calculations = new List<Calculation>
                                                                    {
                                                                        new Calculation
                                                                        {
                                                                            Name = "April 2019-August 2019",
                                                                            Type = Enums.CalculationType.PerPupilFunding,
                                                                            Value = GetData(spreadsheet, 4, la.RowNumber, 2) * 600000,
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
                                                                                    Value = GetData(spreadsheet, 4, la.RowNumber, 2)
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
                                                                            Value = GetData(spreadsheet, 4, la.RowNumber, 3) * 600000,
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
                                                                                    Value = GetData(spreadsheet, 4, la.RowNumber, 3)
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
                                                                    Value = GetDataFromMillions(spreadsheet, 4, la.RowNumber, 7),
                                                                    Calculations = new List<Calculation>
                                                                    {
                                                                        new Calculation
                                                                        {
                                                                            Name = "April 2019-August 2019",
                                                                            Type = Enums.CalculationType.PerPupilFunding,
                                                                            Value = GetData(spreadsheet, 4, la.RowNumber, 5) * 1000000,
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
                                                                                    Value = GetData(spreadsheet, 4, la.RowNumber, 5)
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
                                                                            Value = GetData(spreadsheet, 4, la.RowNumber, 6) * 1000000,
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
                                                                                    Value = GetData(spreadsheet, 4, la.RowNumber, 6)
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
                                                                    Value = GetDataFromMillions(spreadsheet, 4, la.RowNumber, 10),
                                                                    Calculations = new List<Calculation>
                                                                    {
                                                                        new Calculation
                                                                        {
                                                                            Name = "April 2019-August 2019",
                                                                            Type = Enums.CalculationType.PerPupilFunding,
                                                                            Value = GetData(spreadsheet, 4, la.RowNumber, 8) * 240000,
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
                                                                                    Value = GetData(spreadsheet, 4, la.RowNumber, 8)
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
                                                                            Value = GetData(spreadsheet, 4, la.RowNumber, 9) * 240000,
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
                                                                                    Value = GetData(spreadsheet, 4, la.RowNumber, 9)
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
                                                                    Value = GetDataFromMillions(spreadsheet, 4, la.RowNumber, 13),
                                                                    Calculations = new List<Calculation>
                                                                    {
                                                                        new Calculation
                                                                        {
                                                                            Name = "April 2019-August 2019",
                                                                            Type = Enums.CalculationType.PerPupilFunding,
                                                                            Value = GetData(spreadsheet, 4, la.RowNumber, 11) * -1,
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
                                                                                    Value = GetData(spreadsheet, 4, la.RowNumber, 11)
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
                                                                            Value = GetData(spreadsheet, 4, la.RowNumber, 12) * -1,
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
                                                                                    Value = GetData(spreadsheet, 4, la.RowNumber, 12)
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
                                                            Value = GetDataFromMillions(spreadsheet, 4, la.RowNumber, 16) + GetDataFromMillions(spreadsheet, 4, la.RowNumber, 19)
                                                                + GetDataFromMillions(spreadsheet, 4, la.RowNumber, 22),
                                                            FundingLines = new List<FundingLine>
                                                            {
                                                                new FundingLine
                                                                {
                                                                    Name = "Pre-16 SEN Places",
                                                                    Value = GetDataFromMillions(spreadsheet, 4, la.RowNumber, 16),
                                                                    Calculations = new List<Calculation>
                                                                    {
                                                                        new Calculation
                                                                        {
                                                                            Name = "April 2019-August 2019",
                                                                            Type = Enums.CalculationType.PerPupilFunding,
                                                                            Value = GetData(spreadsheet, 4, la.RowNumber, 14) * -1,
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
                                                                                    Value = GetData(spreadsheet, 4, la.RowNumber, 14)
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
                                                                            Value = GetData(spreadsheet, 4, la.RowNumber, 15) * -1,
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
                                                                                    Value = GetData(spreadsheet, 4, la.RowNumber, 15)
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
                                                                    Value = GetDataFromMillions(spreadsheet, 4, la.RowNumber, 19),
                                                                    Calculations = new List<Calculation>
                                                                    {
                                                                        new Calculation
                                                                        {
                                                                            Name = "April 2019-August 2019",
                                                                            Type = Enums.CalculationType.PerPupilFunding,
                                                                            Value = GetData(spreadsheet, 4, la.RowNumber, 17) * -1,
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
                                                                                    Value = GetData(spreadsheet, 4, la.RowNumber, 17)
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
                                                                            Value = GetData(spreadsheet, 4, la.RowNumber, 18) * -1,
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
                                                                                    Value = GetData(spreadsheet, 4, la.RowNumber, 18)
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
                                                                    Value = GetDataFromMillions(spreadsheet, 4, la.RowNumber, 22),
                                                                    Calculations = new List<Calculation>
                                                                    {
                                                                        new Calculation
                                                                        {
                                                                            Name = "April 2019-August 2019",
                                                                            Type = Enums.CalculationType.PerPupilFunding,
                                                                            Value = GetData(spreadsheet, 4, la.RowNumber, 20) * -1,
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
                                                                                    Value = GetData(spreadsheet, 4, la.RowNumber, 20)
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
                                                                            Value = GetData(spreadsheet, 4, la.RowNumber, 21) * -1,
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
                                                                                    Value = GetData(spreadsheet, 4, la.RowNumber, 21)
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
                                                            Value = GetDataFromMillions(spreadsheet, 4, la.RowNumber, 25) + GetDataFromMillions(spreadsheet, 4, la.RowNumber, 28),
                                                            FundingLines = new List<FundingLine>
                                                            {
                                                                new FundingLine
                                                                {
                                                                    Name = "Pre-16 SEN Places",
                                                                    Value = GetDataFromMillions(spreadsheet, 4, la.RowNumber, 25),
                                                                    Calculations = new List<Calculation>
                                                                    {
                                                                        new Calculation
                                                                        {
                                                                            Name = "April 2019-August 2019",
                                                                            Type = Enums.CalculationType.PerPupilFunding,
                                                                            Value = GetData(spreadsheet, 4, la.RowNumber, 23) * -1,
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
                                                                                    Value = GetData(spreadsheet, 4, la.RowNumber, 23)
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
                                                                            Value = GetData(spreadsheet, 4, la.RowNumber, 24) * -1,
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
                                                                                    Value = GetData(spreadsheet, 4, la.RowNumber, 24)
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
                                                                    Value = GetDataFromMillions(spreadsheet, 4, la.RowNumber, 28),
                                                                    Calculations = new List<Calculation>
                                                                    {
                                                                        new Calculation
                                                                        {
                                                                            Name = "April 2019-August 2019",
                                                                            Type = Enums.CalculationType.PerPupilFunding,
                                                                            Value = GetData(spreadsheet, 4, la.RowNumber, 26) * -1,
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
                                                                                    Value = GetData(spreadsheet, 4, la.RowNumber, 26)
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
                                                                            Value = GetData(spreadsheet, 4, la.RowNumber, 27) * -1,
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
                                                                                    Value = GetData(spreadsheet, 4, la.RowNumber, 27)
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
                                                                    Value = GetDataFromMillions(spreadsheet, 4, la.RowNumber, 31),
                                                                    Calculations = new List<Calculation>
                                                                    {
                                                                        new Calculation
                                                                        {
                                                                            Name = "April 2019-August 2019",
                                                                            Type = Enums.CalculationType.PerPupilFunding,
                                                                            Value = GetData(spreadsheet, 4, la.RowNumber, 29) * -1,
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
                                                                                    Value = GetData(spreadsheet, 4, la.RowNumber, 29)
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
                                                                            Value = GetData(spreadsheet, 4, la.RowNumber, 30) * -1,
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
                                                                                    Value = GetData(spreadsheet, 4, la.RowNumber, 30)
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
                                                                    Value = GetDataFromMillions(spreadsheet, 4, la.RowNumber, 34),
                                                                    Calculations = new List<Calculation>
                                                                    {
                                                                        new Calculation
                                                                        {
                                                                            Name = "April 2019-August 2019",
                                                                            Type = Enums.CalculationType.PerPupilFunding,
                                                                            Value = GetData(spreadsheet, 4, la.RowNumber, 32) * -1,
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
                                                                                    Value = GetData(spreadsheet, 4, la.RowNumber, 32)
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
                                                                            Value = GetData(spreadsheet, 4, la.RowNumber, 33) * -1,
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
                                                                                    Value = GetData(spreadsheet, 4, la.RowNumber, 33)
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
                                                            Value = GetDataFromMillions(spreadsheet, 4, la.RowNumber, 37),
                                                            Calculations = new List<Calculation>
                                                            {
                                                                new Calculation
                                                                {
                                                                    Name = "April 2019-August 2019",
                                                                    Type = Enums.CalculationType.PerPupilFunding,
                                                                    Value = GetData(spreadsheet, 4, la.RowNumber, 35) * -1,
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
                                                                            Value = GetData(spreadsheet, 4, la.RowNumber, 35)
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
                                                                    Value = GetData(spreadsheet, 4, la.RowNumber, 36) * -1,
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
                                                                            Value = GetData(spreadsheet, 4, la.RowNumber, 36)
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
                                                            Value = GetDataFromMillions(spreadsheet, 4, la.RowNumber, 40),
                                                            Calculations = new List<Calculation>
                                                            {
                                                                new Calculation
                                                                {
                                                                    Name = "April 2019-August 2019",
                                                                    Type = Enums.CalculationType.PerPupilFunding,
                                                                    Value = GetData(spreadsheet, 4, la.RowNumber, 38) * -1,
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
                                                                            Value = GetData(spreadsheet, 4, la.RowNumber, 38)
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
                                                                    Value = GetData(spreadsheet, 4, la.RowNumber, 39) * -1,
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
                                                                            Value = GetData(spreadsheet, 4, la.RowNumber, 39)
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
                                                            Value = GetDataFromMillions(spreadsheet, 4, la.RowNumber, 43),
                                                            Calculations = new List<Calculation>
                                                            {
                                                                new Calculation
                                                                {
                                                                    Name = "April 2019-August 2019",
                                                                    Type = Enums.CalculationType.LumpSum,
                                                                    Value = GetData(spreadsheet, 4, la.RowNumber, 41) * -1,
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
                                                                            Value = GetData(spreadsheet, 4, la.RowNumber, 41)
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
                                                                    Value = GetData(spreadsheet, 4, la.RowNumber, 42) * -1,
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
                                                                            Value = GetData(spreadsheet, 4, la.RowNumber, 42)
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
                                            Value = GetDataFromMillions(spreadsheet, 1, la.RowNumber, 10),
                                            FundingLines = new List<FundingLine>
                                            {
                                                new FundingLine
                                                {
                                                    Name = "Initial funding funding for universal entitlement for 3 and 4 year olds",
                                                    Value = GetDataFromMillions(spreadsheet, 5, la.RowNumber, 4),
                                                    Calculations = new List<Calculation>
                                                    {
                                                        new Calculation
                                                        {
                                                            Name = "HN Block Before Basic Entitlement and Import and Export Adjustments",
                                                            Type = Enums.CalculationType.PerPupilFunding,
                                                            Value = GetDataFromMillions(spreadsheet, 5, la.RowNumber, 4),
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
                                                                    Value = GetData(spreadsheet, 5, la.RowNumber, 3)
                                                                },
                                                                new ReferenceData
                                                                {
                                                                    Name = "Early Years Universal Entitlement for 3 and 4 Year Olds Rate",
                                                                    Format = Enums.ReferenceDataValueFormat.Number,
                                                                    TemplateReferenceId = templateReferenceId++,
                                                                    Value = GetDataInPence(spreadsheet, 5, la.RowNumber, 2)
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
                                                    Value = GetDataFromMillions(spreadsheet, 5, la.RowNumber, 6),
                                                    Calculations = new List<Calculation>
                                                    {
                                                        new Calculation
                                                        {
                                                            Name = "HN Block Before Basic Entitlement and Import and Export Adjustments",
                                                            Type = Enums.CalculationType.PerPupilFunding,
                                                            Value = GetDataFromMillions(spreadsheet, 5, la.RowNumber, 6),
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
                                                                    Value = GetData(spreadsheet, 5, la.RowNumber, 5)
                                                                },
                                                                new ReferenceData
                                                                {
                                                                    Name = "Early Years Universal Entitlement for 3 and 4 Year Olds Rate",
                                                                    Format = Enums.ReferenceDataValueFormat.Number,
                                                                    TemplateReferenceId = templateReferenceId++,
                                                                    Value = GetDataInPence(spreadsheet, 5, la.RowNumber, 2)
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
                                                    Value = GetDataFromMillions(spreadsheet, 5, la.RowNumber, 9),
                                                    Calculations = new List<Calculation>
                                                    {
                                                        new Calculation
                                                        {
                                                            Name = "Per Pupil Funding for 2 year old entitlement",
                                                            Type = Enums.CalculationType.LumpSum,
                                                            Value =  GetDataFromMillions(spreadsheet, 5, la.RowNumber, 9),
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
                                                                    Value = GetData(spreadsheet, 5, la.RowNumber, 8)
                                                                },
                                                                new ReferenceData
                                                                {
                                                                    Name = "LA hourly rate for 2 year old entitlement",
                                                                    Format = Enums.ReferenceDataValueFormat.Number,
                                                                    TemplateReferenceId = templateReferenceId++,
                                                                    Value = GetDataInPence(spreadsheet, 5, la.RowNumber, 7)
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
                                                    Value = GetDataFromMillions(spreadsheet, 5, la.RowNumber, 10),
                                                    Calculations = new List<Calculation>
                                                    {
                                                        new Calculation
                                                        {
                                                            Name = "Early Years Pupil Premium lumpsum",
                                                            Type = Enums.CalculationType.LumpSum,
                                                            Value = GetDataFromMillions(spreadsheet, 5, la.RowNumber, 10),
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
                                                    Value = GetDataFromMillions(spreadsheet, 5, la.RowNumber, 11),
                                                    Calculations = new List<Calculation>
                                                    {
                                                        new Calculation
                                                        {
                                                            Name = "Disability Access Fund lumpsum",
                                                            Type = Enums.CalculationType.LumpSum,
                                                            Value = GetDataFromMillions(spreadsheet, 5, la.RowNumber, 11),
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
                                                    Value = GetDataFromMillions(spreadsheet, 5, la.RowNumber, 12),
                                                    Calculations = new List<Calculation>
                                                    {
                                                        new Calculation
                                                        {
                                                            Name = "Per Pupil Funding for Maintained Nursery Schools",
                                                            Type = Enums.CalculationType.PerPupilFunding,
                                                            Value = GetDataFromMillions(spreadsheet, 5, la.RowNumber, 12),
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

                // Deep copy this object so we can strip out calcs in it
                //var groupedFundingValue = JsonConvert.DeserializeObject<FundingValue>(JsonConvert.SerializeObject(fundingValue));
                //StripOutCalcsAndInformationLines(groupedFundingValue);

                var data = new LogicalBaseModel
                {
                    SchemaUri = schemaUri,
                    SchemaVersion = schemaVersion,
                    Funding = new FundingProvider
                    {
                        Id = $"{stream.Code}_{period.Code}_{groupingOrg.Type}_{groupingOrg.Identifiers.First(id => id.Type == Enums.OrganisationIdentifierType.UKPRN).Value}_{fundingVersion}",
                        FundingStream = stream,
                        FundingPeriod = period,
                        Status = FundingStatus.Released,
                        GroupingReason = GroupingReason.Payment,
                        FundingVersion = fundingVersion,                        
                        OrganisationGroup = groupingOrg,
                        FundingValue = fundingValue,
                        ProviderFundings = new List<ProviderFunding>
                        {
                            new ProviderFunding
                            {
                                Id = $"{stream.Code}_{period.Code}_{la.UKPRN}_{fundingVersion}",
                                Organisation = new Organisation
                                {
                                    Name = la.Name,
                                    SearchableName = la.Name,
                                    ProviderType = "LA",
                                    ProviderVersionId = "TODO", // TODO
                                    Identifiers = identifiers
                                },
                                FundingPeriodCode = period.Code,
                                FundingStreamCode = stream.Code,
                                FundingValue = fundingValue
                            }
                        },
                        StatusChangedDate = new DateTimeOffset(new DateTime(2019, 3, 1)),
                        ExternalPublicationDate = new DateTimeOffset(new DateTime(2019, 3, 7)),
                        PaymentDate = new DateTimeOffset(new DateTime(2019, 3, 14))
                    },
                };

                returnList.Add(data);
            }

            return returnList.Take(2).ToArray();
        }

        private static void StripOutCalcsAndInformationLines(FundingValue fundingValue)
        {
            foreach (var period in fundingValue.FundingValueByDistributionPeriod)
            {
                var newTopLines = new List<FundingLine>();

                foreach (var line in period.FundingLines)
                {
                    if (line.Type == FundingLineType.Information)
                    {
                        continue;
                    }

                    newTopLines.Add(line);

                    line.Calculations = null;

                    if (line.FundingLines == null)
                    {
                        continue;
                    }

                    var newLines = new List<FundingLine>();

                    foreach (var subline in line.FundingLines)
                    {
                        if (subline.Type == FundingLineType.Information)
                        {
                            continue;
                        }

                        newLines.Add(subline);

                        subline.Calculations = null;

                        if (subline.FundingLines == null)
                        {
                            continue;
                        }

                        var newSubLines = new List<FundingLine>();

                        foreach (var subsubline in subline.FundingLines)
                        {
                            if (subsubline.Type == FundingLineType.Information)
                            {
                                continue;
                            }

                            newSubLines.Add(subsubline);

                            subsubline.Calculations = null;
                        }

                        subline.FundingLines = newSubLines.Count > 0 ? newSubLines : null;
                    }

                    line.FundingLines = newLines.Count > 0 ? newLines : null; ;
                }

                period.FundingLines = newTopLines;
            }
        }

        private class LA
        {
            public string Name { get; set; }

            public string Code { get; set; }

            public string UKPRN { get; set; }

            public int RowNumber { get; set; }
        }
    }
}