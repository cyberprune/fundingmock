using System.Collections.Generic;
using System.Net;
using FundingMock.Web.Models;
using FundingMock.Web.Samples;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;

namespace FundingMock.Web.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    public class ValuesController : ControllerBase
    {
        /// <summary>
        /// Get all fundings as a feed format.
        /// </summary>
        /// <returns>An array of fundings.</returns>
        [HttpGet("api/feed")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IEnumerable<FeedBaseModel>))]
        [SwaggerResponseExample((int)HttpStatusCode.OK, typeof(FeedBaseModelExample))]
        public IActionResult Get()
        {
            return Ok();
        }

        /// <summary>
        /// Get all periods.
        /// </summary>
        /// <returns>An array of periods.</returns>
        [HttpGet("api/periods")]
        [Produces(typeof(IEnumerable<PeriodExtended>))]
        public IActionResult GetPeriods()
        {
            return Ok();
        }

        /// <summary>
        /// Get all funding streams.
        /// </summary>
        /// <returns>An array of funding streams.</returns>
        [HttpGet("api/fundingstreams")]
        [Produces(typeof(IEnumerable<StreamExtended>))]
        public IActionResult GetFundingStreams()
        {
            return Ok();
        }
    }
}