using System;

namespace FundingMock.Web.Models
{
    public class FeedResponseContentModel
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public FeedResponseAuthor Author { get; set; }

        public DateTime Updated { get; set; }
        
        public FeedLink[] Link { get; set; }

        public FeedBaseModel Content { get; set; }
    }
}