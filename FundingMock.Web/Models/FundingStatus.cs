using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FundingMock.Web.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum FundingStatus
    {
        Published,
    }
}