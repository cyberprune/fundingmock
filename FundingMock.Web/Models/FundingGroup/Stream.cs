namespace FundingMock.Web.Models
{
    /// <summary>
    /// Details around a funding stream.
    /// </summary>
    public class Stream
    {
        /// <summary>
        /// The code for the funding stream (e.g. PESport).
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// The name of the funding stream (e.g. PE Sport &amp; Premium).
        /// </summary>
        public string Name { get; set; }
    }
}