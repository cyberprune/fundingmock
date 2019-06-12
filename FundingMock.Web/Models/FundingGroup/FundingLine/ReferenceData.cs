using FundingMock.Web.Enums;
using System.ComponentModel.DataAnnotations;

namespace FundingMock.Web.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class ReferenceData
    {
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [EnumDataType(typeof(ValueFormat))]
        public ValueFormat Format { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal Value { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Sequence { get; set; }
    }
}