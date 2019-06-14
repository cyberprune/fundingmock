namespace FundingMock.Web.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class StreamWithTemplateVersion : Stream
    {
        /// <summary>
        /// The version of the template (e.g. this is Version 2 of PE and sport template).
        /// </summary>
        public string TemplateVersion { get; set; }
    }
}