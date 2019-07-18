using System.Collections.Generic;
using System.IO.Compression;
using System.Net;
using System.Text;
using FundingMock.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using System.Linq;
using System;
using FundingMock.Web.Enums;
using Sfa.Sfs.Mock.Generators;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.RegularExpressions;
using FundingMock.Web.Examples;

namespace FundingMock.Web.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    public class FundingController : ControllerBase
    {
        /// <summary>
        /// Get all fundings in a feed.
        /// </summary>
        /// <param name="pageSize">Optional - The maximum number of records to return.</param>
        /// <param name="fundingPeriodStartYear">Optional - The period start year to limit to</param>
        /// <param name="fundingPeriodEndYear">Optional - The period end year to limit to</param>
        /// <param name="fundingPeriodCodes">Optional - The period codes to limit to (e.g. AY1920).</param>
        /// <param name="organisationGroupIdentifiers">Optional - The group identifiers to limit by (e.g. UKPRN 12345678).</param>
        /// <param name="organisationGroupTypes">Optional - The group types to limit to (e.g. Region, LocalAuthority).</param>
        /// <param name="organisationIdentifiers">Optional - The organisation identifiers to limit to (e.g. UKPRN 12345678).</param>
        /// <param name="organisationTypes">Optional - The organisation types to return.</param>
        /// <param name="variationReasons">Optional - Filter to only organisations with these variation reasons types</param>
        /// <param name="ukprns">Optional - Only get these UKPRNs back.</param>
        /// <param name="groupingReasons">Optional - The grouping reasons we want to get back (e.g. Information and/or Payment).</param>
        /// <param name="statuses">Optional - The status of the funding (e.g. Released).</param>
        /// <param name="minStatusChangeDate">Optional - Only get records back that were changed after this date.</param>
        /// <param name="fundingStreamCodes">Optional - limit by these stream types (e.g. DSG, PESports)).</param>
        /// <param name="fundingLineTypes">Optional - limit the types of lines we want to get back (e.g. Information and/or Payment).</param>
        /// <param name="templateLineIds">Optional - Filter the lines to these ids only.</param>
        /// <returns>An array of feed results.</returns>
        [HttpGet("api/feed")]
        [SwaggerRequestExample(typeof(FeedRequestObject), typeof(FeedRequestExample))]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(FeedResponseModel))]
        [SwaggerResponseExample((int)HttpStatusCode.OK, typeof(FeedResponseModelExample))]
        public IActionResult GetFeed(int? pageSize = null, int? fundingPeriodStartYear = null,
            int? fundingPeriodEndYear = null, string[] fundingPeriodCodes = null,
            OrganisationIdentifier[] organisationGroupIdentifiers = null, OrganisationGroupTypeIdentifier[] organisationGroupTypes = null,
            OrganisationIdentifier[] organisationIdentifiers = null, OrganisationGroupTypeIdentifier[] organisationTypes = null,
            VariationReason[] variationReasons = null, string[] ukprns = null, GroupingReason[] groupingReasons = null,
            FundingStatus[] statuses = null, DateTime? minStatusChangeDate = null, string[] fundingStreamCodes = null,
            FundingLineType[] fundingLineTypes = null, string[] templateLineIds = null)
        {
            return GetFeedPage(null, pageSize, fundingPeriodStartYear, fundingPeriodEndYear, fundingPeriodCodes, organisationGroupIdentifiers,
                organisationGroupTypes, organisationIdentifiers, organisationTypes, variationReasons, ukprns, groupingReasons, statuses,
                minStatusChangeDate, fundingStreamCodes, fundingLineTypes, templateLineIds);
        }

        /// <summary>
        /// Get all fundings as a feed format (after newest page).
        /// </summary>
        /// <param name="pageRef">Optional - the page to look at.</param>
        /// <param name="pageSize">Optional - The maximum number of records to return.</param>
        /// <param name="fundingPeriodStartYear">Optional - The period start year to limit to</param>
        /// <param name="fundingPeriodEndYear">Optional - The period end year to limit to</param>
        /// <param name="fundingPeriodCodes">Optional - The period codes to limit to (e.g. AY1920).</param>
        /// <param name="organisationGroupIdentifiers">Optional - The group identifiers to limit by (e.g. UKPRN 12345678).</param>
        /// <param name="organisationGroupTypes">Optional - The group types to limit to (e.g. Region, LocalAuthority).</param>
        /// <param name="organisationIdentifiers">Optional - The organisation identifiers to limit to (e.g. UKPRN 12345678).</param>
        /// <param name="organisationTypes">Optional - The organisation types to return.</param>
        /// <param name="variationReasons">Optional - Filter to only organisations with these variation reasons types</param>
        /// <param name="ukprns">Optional - Only get these UKPRNs back.</param>
        /// <param name="groupingReasons">Optional - The grouping reasons we want to get back (e.g. Information and/or Payment).</param>
        /// <param name="statuses">Optional - The status of the funding (e.g. Released).</param>
        /// <param name="minStatusChangeDate">Optional - Only get records back that were changed after this date.</param>
        /// <param name="fundingStreamCodes">Optional - limit by these stream types (e.g. DSG, PESports)).</param>
        /// <param name="fundingLineTypes">Optional - limit the types of lines we want to get back (e.g. Information and/or Payment).</param>
        /// <param name="templateLineIds">Optional - Filter the lines to these ids only.</param>
        /// <returns>An array of feed results.</returns>
        [HttpGet("api/feed/{pageRef}")]
        [SwaggerRequestExample(typeof(FeedRequestObject), typeof(FeedRequestExample))]
        [SwaggerResponse((int)HttpStatusCode.OK, Type = typeof(FeedResponseModel))]
        [SwaggerResponseExample((int)HttpStatusCode.OK, typeof(FeedResponseModelExample))]
        public IActionResult GetFeedPage([FromRoute] int? pageRef, int? pageSize = null, int? fundingPeriodStartYear = null,
            int? fundingPeriodEndYear = null, string[] fundingPeriodCodes = null,
            OrganisationIdentifier[] organisationGroupIdentifiers = null, OrganisationGroupTypeIdentifier[] organisationGroupTypes = null,
            OrganisationIdentifier[] organisationIdentifiers = null, OrganisationGroupTypeIdentifier[] organisationTypes = null,
            VariationReason[] variationReasons = null, string[] ukprns = null, GroupingReason[] groupingReasons = null,
            FundingStatus[] statuses = null, DateTime? minStatusChangeDate = null, string[] fundingStreamCodes = null,
            FundingLineType[] fundingLineTypes = null, string[] templateLineIds = null)
        {
            var schemeAndHost = "http://mock.web";

            var fullList = new List<FeedResponseContentModel>();
            var pageSizeT = pageSize ?? 10;

            if (fundingStreamCodes?.Any() != true || fundingStreamCodes.Contains("DSG"))
            {
                fullList.AddRange(GenerateDSGFunding.GenerateFeed(fundingPeriodStartYear, fundingPeriodEndYear, fundingPeriodCodes, organisationGroupIdentifiers,
                    organisationGroupTypes, organisationIdentifiers, organisationTypes, variationReasons, ukprns, groupingReasons, statuses,
                    minStatusChangeDate, fundingLineTypes, templateLineIds)
                );
            }

            if (fundingStreamCodes?.Any() != true || fundingStreamCodes.Contains("PESports"))
            {
                fullList.AddRange(GeneratePESportsFunding.GenerateFeed(fundingPeriodStartYear, fundingPeriodEndYear, fundingPeriodCodes,
                    organisationGroupIdentifiers, organisationGroupTypes, organisationIdentifiers, organisationTypes, variationReasons, ukprns,
                    groupingReasons, statuses, minStatusChangeDate, fundingLineTypes, templateLineIds));
            }

            fullList.Reverse();

            var limitedList = fullList.Skip(((pageRef ?? 1) - 1) * pageSizeT).Take(pageSizeT).ToArray();
            limitedList.Reverse(); // Reverse back

            var lastPageNumber = 1;
            var firstPageNumber = (int)Math.Ceiling(fullList.Count / (double)pageSizeT);
            var currentPageNumber = pageRef ?? firstPageNumber;
            var isFirstPage = firstPageNumber == currentPageNumber;
            var isLastPage = currentPageNumber == lastPageNumber;

            var qs = Request.QueryString.ToString();

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
                Link = new List<FeedLink>
                {
                    new FeedLink
                    {
                        Href = isFirstPage ? $"{schemeAndHost}/api/funding/feed" :
                            $"{schemeAndHost}/api/funding/feed/{currentPageNumber}{qs}",
                        Rel = "self"
                    },
                    new FeedLink
                    {
                        Href = $"{schemeAndHost}/api/funding/feed/{firstPageNumber}{qs}",
                        Rel = "first"
                    },
                    new FeedLink
                    {
                        Href = $"{schemeAndHost}/api/funding/feed/{lastPageNumber}{qs}",
                        Rel = "last"
                    }
                },
                AtomEntry = limitedList.ToArray()
            };

            if (!isFirstPage)
            {
                returnItem.Link.Add(new FeedLink
                {
                    Href = $"{schemeAndHost}/api/funding/feed/{currentPageNumber + 1}{qs}",
                    Rel = "next"
                });
            }

            if (!isLastPage)
            {
                returnItem.Link.Add(new FeedLink
                {
                    Href = $"{schemeAndHost}/api/funding/feed/{currentPageNumber - 1}{qs}",
                    Rel = "previous"
                });
            }

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
                FeedBaseModel data = null;

                if (parts[0].Equals("DSG", StringComparison.InvariantCultureIgnoreCase))
                {
                    data = GenerateDSGFunding.GetFeedEntry(id);
                }
                else if (parts[0].Equals("PESports", StringComparison.InvariantCultureIgnoreCase))
                {
                    data = GeneratePESportsFunding.GetFeedEntry(id);
                }

                return data == null ? NotFound() : (IActionResult)Ok(data);
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
            ProviderFunding data = null;

            if (providerFundingFeedId.StartsWith("DSG_"))
            {
                data = GenerateDSGFunding.GenerateProviderFunding(providerFundingFeedId);
            }
            else if (providerFundingFeedId.StartsWith("PESports_"))
            {
                data = GeneratePESportsFunding.GenerateProviderFunding(providerFundingFeedId);
            }

            return data == null ? NotFound() : (IActionResult)Ok(data);
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
        public HttpResponseMessage DownloadDSGFiles()
        {
            var takeAtOnce = 10;
            var processedFileNames = new List<string>();

            byte[] fileBytes = null;
            ZipArchiveEntry zipItem;

            using (var memoryStream = new System.IO.MemoryStream())
            {
                var feed = GenerateDSGFunding.GenerateFeed(null, null, null, null, null, null, null, null, null, null, null, null, null, null);

                using (var zip = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    for (var idx = 0; idx <= (int)Math.Ceiling(feed.Length / 10.0); idx++)
                    {
                        var feedResponse = feed.Skip(idx * 10).Take(10).ToArray();

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

            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(fileBytes)
            };

            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "allDsg.zip"
            };

            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");

            return result;
        }

        /// <summary>
        /// Helper function to download all PESports files as a zip (very slow).
        /// </summary>
        /// <returns>Nothing - but writes to disk.</returns>
        [HttpGet("api/downloadPesportsFiles")]
        public HttpResponseMessage DownloadPESportsFiles()
        {
            var takeAtOnce = 50;
            var processedFileNames = new List<string>();

            byte[] fileBytes = null;
            ZipArchiveEntry zipItem;

            using (var memoryStream = new System.IO.MemoryStream())
            {
                using (var zip = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    var feedResponseFull = GeneratePESportsFunding.GenerateFeed(null, null, null, null, null, null, null, null, null, null, null, null, null, null);

                    for (var idx = 0; idx <= (int)Math.Ceiling(feedResponseFull.Length / 10.0); idx++)
                    {
                        var feedResponse = feedResponseFull.Skip(idx * 10).Take(10).ToArray();

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

            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(fileBytes)
            };

            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "allPesports.zip"
            };

            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");

            return result;
        }

        // Regular Expressions for processing the search text:
        private static readonly Regex _spacingCharacters = new Regex(@"[\s\u2212\u2013\u2014\u2010-]+", RegexOptions.Compiled);
        private static readonly Regex _disallowedCharacters = new Regex(@"[^\w]+", RegexOptions.Compiled);
        private static readonly Regex _multipleDashes = new Regex(@"_{2,}", RegexOptions.Compiled);

        /// <summary>
        /// Sanitise the name by removing unallowed characters, spaces etc.... Must match how the name is sanitised in the front end.
        /// </summary>
        /// <param name="originalSearchTerm">The unsanitised name of the provider/la.</param>
        /// <returns>A string in the format we require.</returns>
        public static string SanitiseName(string originalSearchTerm)
        {
            var defaultName = string.Empty;

            if (string.IsNullOrEmpty(originalSearchTerm))
            {
                return defaultName;
            }

            var cleanSearchTerm = _spacingCharacters.Replace(originalSearchTerm, "_");
            cleanSearchTerm = _disallowedCharacters.Replace(cleanSearchTerm, string.Empty);
            cleanSearchTerm = _multipleDashes.Replace(cleanSearchTerm, "_");
            cleanSearchTerm = cleanSearchTerm.Trim(new[] { '_' });

            if (cleanSearchTerm == string.Empty)
            {
                return defaultName;
            }

            return cleanSearchTerm;
        }
    }
}