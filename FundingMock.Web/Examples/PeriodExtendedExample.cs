using System;
using System.Collections.Generic;
using FundingMock.Web.Enums;
using FundingMock.Web.Models;
using Swashbuckle.AspNetCore.Filters;

namespace FundingMock.Web.Examples
{
    public class PeriodExtendedExample : IExamplesProvider
    {
        public object GetExamples()
        {
            return new List<PeriodExtended>
            {
                new PeriodExtended
                {
                    Period = "AY1920",
                    Name = "Academic Year 2019 to 2020",
                    Type = Enums.FundingPeriodType.AY,
                    StartDate = new DateTimeOffset(2019, 9, 1, 0, 0, 0, new TimeSpan(0, 0, 0)),
                    EndDate = new DateTimeOffset(2020, 8, 31, 0, 0, 0, new TimeSpan(0, 0, 0))
                },
                new PeriodExtended
                {
                    Period = "AY2021",
                    Name = "Academic Year 2020 to 2021",
                    Type = Enums.FundingPeriodType.AY,
                    StartDate = new DateTimeOffset(2020, 9, 1, 0, 0, 0, new TimeSpan(0, 0, 0)),
                    EndDate = new DateTimeOffset(2021, 8, 31, 0, 0, 0, new TimeSpan(0, 0, 0))
                },
                new PeriodExtended
                {
                    Period = "FY1920",
                    Name = "Financial Year 2019 to 2020",
                    Type = Enums.FundingPeriodType.FY,
                    StartDate = new DateTimeOffset(2019, 4, 1, 0, 0, 0, new TimeSpan(0, 0, 0)),
                    EndDate = new DateTimeOffset(2020, 3, 30, 0, 0, 0, new TimeSpan(0, 0, 0))
                },
                new PeriodExtended
                {
                    Period = "FY2021",
                    Name = "Financial Year 2020 to 2021",
                    Type = Enums.FundingPeriodType.FY,
                    StartDate = new DateTimeOffset(2020, 4, 1, 0, 0, 0, new TimeSpan(0, 0, 0)),
                    EndDate = new DateTimeOffset(2021, 3, 30, 0, 0, 0, new TimeSpan(0, 0, 0))
                }
            };
        }
    }
}