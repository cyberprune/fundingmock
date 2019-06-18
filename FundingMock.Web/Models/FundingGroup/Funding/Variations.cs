using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FundingMock.Web.Enums;

namespace FundingMock.Web.Models
{
    public class Variations
    {
        /// <summary>
        /// Optional reasons for the provider variation. These reasons are in addition to open and close reason of the organisation.
        /// This field can contain zero or more items.
        /// </summary>
        [EnumDataType(typeof(VariationReason))]
        public IEnumerable<VariationReason> VariationReasons { get; set; }

        /// <summary>
        /// Collection of successor providers
        /// </summary>
        public IEnumerable<ProviderInformationModel> Successors { get; set; }

        /// <summary>
        /// Collection of predecessor providers
        /// </summary>
        public IEnumerable<ProviderInformationModel> Predecessors { get; set; }
    }
}