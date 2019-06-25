using System;

namespace FundingMock.Web.Models
{
    public class FeedResponseModel
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public FeedResponseAuthor Author { get; set; }

        public DateTime Updated { get; set; }

        public string Rights { get; set; }

        public FeedLink[] Link { get; set; }

        public FeedResponseContentModel[] AtomEntry { get; set; }
    }
}