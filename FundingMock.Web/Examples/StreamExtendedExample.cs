using System;
using System.Collections.Generic;
using FundingMock.Web.Enums;
using FundingMock.Web.Models;
using Swashbuckle.AspNetCore.Filters;

namespace FundingMock.Web.Examples
{
    public class StreamExtendedExample : IExamplesProvider
    {
        public object GetExamples()
        {
            return new List<StreamExtended>
            {
                new StreamExtended
                {
                    Code = "PESports",
                    Name = "PE + Sport Premium"
                },
                new StreamExtended
                {
                    Code = "DSG",
                    Name = "Dedicated Schools Grant"
                },
                new StreamExtended
                {
                    Code = "GAG",
                    Name = "Academy General Annual Grant"
                }
            };
        }
    }
}