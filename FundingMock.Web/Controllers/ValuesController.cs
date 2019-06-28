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
using System.Linq;
using System;

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
        /// <param name="feedRequestObject">Optional - feed request parameters.</param>
        /// <returns>An array of feed results.</returns>
        [HttpPost("api/feed")]
        [SwaggerRequestExample(typeof(FeedRequestObject), typeof(FeedRequestExample))]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(FeedResponseModel))]
        [SwaggerResponseExample((int)HttpStatusCode.OK, typeof(FeedResponseModelExample))]
        public IActionResult GetFeed(FeedRequestObject feedRequestObject = null)
        {
            return GetFeedPage(null, feedRequestObject);
        }

        /// <summary>
        /// Get all fundings as a feed format (after newest page).
        /// </summary>
        /// <param name="pageRef">Optional - the page to look at.</param>
        /// <param name="feedRequestObject">Optional - feed request parameters.</param>
        /// <returns>An array of feed results.</returns>
        [HttpPost("api/feed/{pageRef}")]
        [SwaggerRequestExample(typeof(FeedRequestObject), typeof(FeedRequestExample))]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(FeedResponseModel))]
        [SwaggerResponseExample((int)HttpStatusCode.OK, typeof(FeedResponseModelExample))]
        public IActionResult GetFeedPage([FromRoute] int? pageRef, FeedRequestObject feedRequestObject = null)
        {
            var list = new List<FeedResponseContentModel>();
            var pageSize = feedRequestObject?.PageSize ?? 10;

            if (feedRequestObject?.FundingStreamCodes == null || feedRequestObject.FundingStreamCodes.Contains("DSG"))
            {
                list.AddRange(GenerateDSGFunding.GenerateFeed(feedRequestObject, pageSize, pageRef));
            }

            if (feedRequestObject?.FundingStreamCodes == null || feedRequestObject.FundingStreamCodes.Contains("PESports"))
            {
                list.AddRange(GeneratePESportsFunding.GenerateFeed(pageSize, pageRef));
            }

            var returnItem = new FeedResponseModel
            {
                Id = Guid.NewGuid().ToString(),
                Title = "MYESF / CFS shared funding model mock API",
                Author = new FeedResponseAuthor
                {
                    Email = "calculate-funding@education.gov.uk",
                    Name = "Calculate Funding Service"
                },
                Updated = DateTime.Now,
                Rights = "Copyright (C) 2019 Department for Education",
                Link = new FeedLink[]
                {
                    new FeedLink
                    {
                        Href = "#",
                        Rel = "self"
                    }
                },
                AtomEntry = list.ToArray()
            };

            return Ok(returnItem);
        }

        /// <summary>
        /// Get a funding by id.
        /// </summary>
        /// <param name="id">The ID to look up.</param>
        /// <returns>An array of feed results.</returns>
        [HttpGet("api/feed/byId/{id}")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(FeedBaseModel))]
        [SwaggerResponseExample((int)HttpStatusCode.OK, typeof(FeedBaseModelExample))]
        public IActionResult GetFeedById(string id)
        {
            var parts = id?.Split('_');

            if (parts == null || parts.Length == 0)
            {
                return NotFound();
            }

            if (parts[0].Equals("DSG", StringComparison.InvariantCultureIgnoreCase))
            {
                var data = GenerateDSGFunding.GetFeedEntry(id);

                if (data == null)
                {
                    return NotFound();
                }

                return Ok(data);
            }
            else if (parts[0].Equals("PESports", StringComparison.InvariantCultureIgnoreCase))
            {
                var data = GeneratePESportsFunding.GetFeedEntry(id);

                if (data == null)
                {
                    return NotFound();
                }

                return Ok(data);
            }

            return NotFound();
        }

        /// <summary>
        /// Get provider funding detail. 
        /// </summary>
        /// <param name="providerFundingFeedId">The ID to look up.</param>
        /// <returns>An array of fundings.</returns>
        [HttpGet("api/providerfunding/{providerFundingFeedId}")]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(ProviderFunding))]
        [SwaggerResponseExample((int)HttpStatusCode.OK, typeof(ProviderFundingModelExample))]
        public IActionResult GetProviderFunding(string providerFundingFeedId)
        {
            if (providerFundingFeedId.StartsWith("DSG_"))
            {
                var data = GenerateDSGFunding.GenerateProviderFunding(providerFundingFeedId);
                return Ok(data);
            }
            else if (providerFundingFeedId.StartsWith("PESports_"))
            {
                var data = GeneratePESportsFunding.GenerateProviderFunding(providerFundingFeedId);
                return Ok(data);
            }

            return NotFound();
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
        /// Helper function to download all DSG files as a zip.
        /// </summary>
        /// <returns>Nothing - but writes to disk.</returns>
        [HttpGet("api/downloadDsgFiles")]
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
                        var feedResponse = GenerateDSGFunding.GenerateFeed(null, takeAtOnce, idx * takeAtOnce);

                        foreach (var feedEntry in feedResponse)
                        {
                            foreach (var providerFundingId in feedEntry.Content.Funding.ProviderFundings)
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
        /// Helper function to download all PESports files as a zip (very slow).
        /// </summary>
        /// <returns>Nothing - but writes to disk.</returns>
        [HttpGet("api/downloadPesportsFiles")]
        public IActionResult DownloadPESportsFiles()
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
                        var feedResponse = GeneratePESportsFunding.GenerateFeed(takeAtOnce, idx * takeAtOnce);

                        foreach (var feedEntry in feedResponse)
                        {
                            foreach (var providerFundingId in feedEntry.Content.Funding.ProviderFundings)
                            {
                                var fileName = $"{providerFundingId}.txt";

                                if (processedFileNames.Contains(fileName))
                                {
                                    continue;
                                }

                                var funding = GeneratePESportsFunding.GenerateProviderFunding(providerFundingId);
                                var fundingStr = JsonConvert.SerializeObject(funding);

                                zipItem = zip.CreateEntry(fileName);
                                processedFileNames.Add(fileName);

                                using (var originalFileMemoryStream = new System.IO.MemoryStream(Encoding.ASCII.GetBytes(fundingStr)))
                                {
                                    using (var entryStream = zipItem.Open())
                                    {
                                        originalFileMemoryStream.CopyTo(entryStream);
                                    }
                                }
                            }
                        }

                        zipItem = zip.CreateEntry($"FeedResponse_PESports_{(idx * takeAtOnce) + 1}-{(idx * takeAtOnce) + feedResponse.Length}.txt");
                        var feedResponseStr = JsonConvert.SerializeObject(feedResponse);

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

            Response.Headers.Add("Content-Disposition", "attachment; filename=allPesports.zip");
            return File(fileBytes, "application/zip");
        }
    }
}