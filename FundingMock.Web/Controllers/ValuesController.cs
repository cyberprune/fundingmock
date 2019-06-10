using System.Collections.Generic;
using FundingMock.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace FundingMock.Web.Controllers
{
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [HttpGet("api/feed")]
        [Produces(typeof(BaseModel))]
        public IActionResult GetFeed()
        {
            return Ok();
        }

        [HttpGet("api/periods")]
        [Produces(typeof(IEnumerable<FundingPeriod>))]
        public IActionResult GetFundingperiods()
        {
            return Ok();
        }

        [HttpGet("api/fundingstreams")]
        [Produces(typeof(IEnumerable<FundingStream>))]
        public IActionResult GetFundingStreams()
        {
            return Ok();
        }




        //// POST api/values
        //[HttpPost]
        //public void Post([FromBody] string value)
        //{
        //}

        //// PUT api/values/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE api/values/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
