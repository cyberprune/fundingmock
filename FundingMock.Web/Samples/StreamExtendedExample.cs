using FundingMock.Web.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Collections.Generic;

namespace FundingMock.Web.Samples
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