using System.Collections.Generic;
using FundingMock.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace FundingMock.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [HttpGet("fundings")]
        [Produces(typeof(IEnumerable<BaseModel>))]
        public IActionResult Get()
        {
            return Ok();
        }

        [HttpGet("{periods}")]
        [Produces(typeof(IEnumerable<FundingPeriod>))]
        public IActionResult GetFundingperiods()
        {
            return Ok();
        }

        [HttpGet("{fundingstreams}")]
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
