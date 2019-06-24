using System.Collections.Generic;
using System.IO.Compression;
using System.Net;
using System.Text;
using FundingMock.Web.Models;
using FundingMock.Web.Samples;
using FundingMock.Web.Tools;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
        /// <param name="streams">Limit to certain steams if passed - if null or empty, all streams are returned. Use commas to seperate (e.g. 'PESP,DSG').</param>
        /// <param name="groupingTypes">Limit to certain grouping types (LA, Region) if passed - if null or empty, all grouping types are returned.</param>
        /// <param name="filterBy">Filter by a specific string (this can be LA code or region name, for example).</param>
        /// <param name="maxResults">Max results to get - default is 10.</param>
        /// <param name="skip">Number of results to skip - default is 0.</param>
        /// <returns>An array of feed results.</returns>
        [HttpGet("api/feed")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IEnumerable<FeedBaseModel>))]
        [SwaggerResponseExample((int)HttpStatusCode.OK, typeof(FeedBaseModelExample))]
        public IActionResult Get(string streams, string groupingTypes, string filterBy, int maxResults = 10, int skip = 0)
        {
            var returnList = new List<FeedBaseModel>();

            if (string.IsNullOrEmpty(streams) || streams.ToLower().Contains("dsg"))
            {
                returnList.AddRange(GenerateDSGFunding.GenerateFeed(groupingTypes, filterBy, maxResults, skip));
            }

            if (string.IsNullOrEmpty(streams) || streams.ToLower().Contains("pesp"))
            {
                returnList.AddRange(GeneratePESPFunding.GenerateFeed(maxResults, skip));
            }

            return Ok(returnList);
        }

        /// <summary>
        /// Get all fundings as a feed format - full logical format, this API won't be produced, it's purely for documentation
        /// </summary>
        /// <returns>An array of fundings.</returns>
        [HttpGet("api/logical")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IEnumerable<LogicalBaseModel>))]
        [SwaggerResponseExample((int)HttpStatusCode.OK, typeof(LogicalBaseModelExample))]
        public IActionResult GetLogicalFeed()
        {
            return Ok(new LogicalBaseModelExample().GetExamples());
        }

        /// <summary>
        /// Get all fundings as a feed format.
        /// </summary>
        /// <param name="providerFundingFeedId">The ID to look up.</param>
        /// <returns>An array of fundings.</returns>
        [HttpGet("api/provider/{providerFundingFeedId}")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(ProviderFunding))]
        [SwaggerResponseExample((int)HttpStatusCode.OK, typeof(ProviderFundingModelExample))]
        public IActionResult GetProviderFunding(string providerFundingFeedId)
        {
            if (providerFundingFeedId.StartsWith("DSG_"))
            {
                return Ok(GenerateDSGFunding.GenerateProviderFunding(providerFundingFeedId));
            }
            else if (providerFundingFeedId.StartsWith("PESports_"))
            {
                return Ok(GeneratePESPFunding.GenerateProviderFunding(providerFundingFeedId));
            }

            return Ok();
        }

        /// <summary>
        /// Get all periods.
        /// </summary>
        /// <returns>An array of periods.</returns>
        [HttpGet("api/periods")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IEnumerable<PeriodExtended>))]
        [SwaggerResponseExample((int)HttpStatusCode.OK, typeof(PeriodExtendedExample))]
        public IActionResult GetPeriods()
        {
            return Ok(new PeriodExtendedExample().GetExamples());
        }

        /// <summary>
        /// Get all funding streams.
        /// </summary>
        /// <returns>An array of funding streams.</returns>
        [HttpGet("api/fundingstreams")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(IEnumerable<StreamExtended>))]
        [SwaggerResponseExample((int)HttpStatusCode.OK, typeof(StreamExtendedExample))]
        public IActionResult GetFundingStreams()
        {
            return Ok(new StreamExtendedExample().GetExamples());
        }

        /// <summary>
        /// Helper function to download all DSG files as a zip.
        /// </summary>
        /// <returns>Nothing - but writes to disk.</returns>
        [HttpGet("api/downloadDSGfiles")]
        public IActionResult DownloadDSGFiles()
        {
            var takeAtOnce = 10;
            var processedFileNames = new List<string>();

            byte[] fileBytes = null;
            ZipArchiveEntry zipItem;

            using (var memoryStream = new System.IO.MemoryStream())
            {
                using (var zip = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    for (var idx = 0; idx <= 16; idx++)
                    {
                        var feedResponse = GenerateDSGFunding.GenerateFeed(string.Empty, string.Empty, takeAtOnce, idx * takeAtOnce);

                        foreach (var feedEntry in feedResponse)
                        {
                            foreach (var providerFundingId in feedEntry.Funding.ProviderFundings)
                            {
                                var fileName = $"{providerFundingId}.txt";

                                if (processedFileNames.Contains(fileName))
                                {
                                    continue;
                                }

                                var funding = GenerateDSGFunding.GenerateProviderFunding(providerFundingId);
                                var fundingStr = JsonConvert.SerializeObject(funding);

                                zipItem = zip.CreateEntry(fileName);
                                processedFileNames.Add(fileName);

                                // add the item bytes to the zip entry by opening the original file and copying the bytes 
                                using (var originalFileMemoryStream = new System.IO.MemoryStream(Encoding.ASCII.GetBytes(fundingStr)))
                                {
                                    using (var entryStream = zipItem.Open())
                                    {
                                        originalFileMemoryStream.CopyTo(entryStream);
                                    }
                                }
                            }
                        }

                        zipItem = zip.CreateEntry($"FeedResponse{(idx * takeAtOnce) + 1}-{(idx * takeAtOnce) + feedResponse.Length}.txt");
                        var feedResponseStr = JsonConvert.SerializeObject(feedResponse);

                        // add the item bytes to the zip entry by opening the original file and copying the bytes 
                        using (var originalFileMemoryStream = new System.IO.MemoryStream(Encoding.ASCII.GetBytes(feedResponseStr)))
                        {
                            using (var entryStream = zipItem.Open())
                            {
                                originalFileMemoryStream.CopyTo(entryStream);
                            }
                        }
                    }
                }

                fileBytes = memoryStream.ToArray();
            }

            Response.Headers.Add("Content-Disposition", "attachment; filename=allDsg.zip");
            return File(fileBytes, "application/zip");
        }

        /// <summary>
        /// Helper function to download all PESP files as a zip (very slow).
        /// </summary>
        /// <returns>Nothing - but writes to disk.</returns>
        [HttpGet("api/downloadPESPfiles")]
        public IActionResult DownloadPESPFiles()
        {
            var takeAtOnce = 50;
            var processedFileNames = new List<string>();

            byte[] fileBytes = null;
            ZipArchiveEntry zipItem;

            using (var memoryStream = new System.IO.MemoryStream())
            {
                using (var zip = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    for (var idx = 0; idx <= 120; idx++)
                    {
                        var feedResponse = GeneratePESPFunding.GenerateFeed(takeAtOnce, idx * takeAtOnce);

                        foreach (var feedEntry in feedResponse)
                        {
                            foreach (var providerFundingId in feedEntry.Funding.ProviderFundings)
                            {
                                var fileName = $"{providerFundingId}.txt";

                                if (processedFileNames.Contains(fileName))
                                {
                                    continue;
                                }

                                var funding = GeneratePESPFunding.GenerateProviderFunding(providerFundingId);
                                var fundingStr = JsonConvert.SerializeObject(funding);

                                zipItem = zip.CreateEntry(fileName);
                                processedFileNames.Add(fileName);

                                // add the item bytes to the zip entry by opening the original file and copying the bytes 
                                using (var originalFileMemoryStream = new System.IO.MemoryStream(Encoding.ASCII.GetBytes(fundingStr)))
                                {
                                    using (var entryStream = zipItem.Open())
                                    {
                                        originalFileMemoryStream.CopyTo(entryStream);
                                    }
                                }
                            }
                        }

                        zipItem = zip.CreateEntry($"FeedResponse_PESP_{(idx * takeAtOnce) + 1}-{(idx * takeAtOnce) + feedResponse.Count}.txt");
                        var feedResponseStr = JsonConvert.SerializeObject(feedResponse);

                        // add the item bytes to the zip entry by opening the original file and copying the bytes 
                        using (var originalFileMemoryStream = new System.IO.MemoryStream(Encoding.ASCII.GetBytes(feedResponseStr)))
                        {
                            using (var entryStream = zipItem.Open())
                            {
                                originalFileMemoryStream.CopyTo(entryStream);
                            }
                        }
                    }
                }

                fileBytes = memoryStream.ToArray();
            }

            Response.Headers.Add("Content-Disposition", "attachment; filename=allPESP.zip");
            return File(fileBytes, "application/zip");
        }
    }
}