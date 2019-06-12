using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FundingMock.Web.Models
{
    /// <summary>
    /// A funding group (a parent grouping organisation - such as an LA, MAT, Region etc...).
    /// </summary>
    public class FundingGroup
    {
        /// <summary>
        /// Unique identifier of this business event (e.g. GAG-1920-ACADEMIES ENTERPRISE TRUST-version1).
        /// QUESTION - How is this built up? "PE1920-Camden-v1" doesnt seem to be a composition of fields we can access
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The version of the template (e.g. this is Version 2 of PE and sport template).
        /// </summary>
        public int TemplateVersion { get; set; }

        /// <summary>
        /// Version number of the published data. If there are changes to the funding for this organisation in this period, this number would increase.
        /// </summary>
        public int FundingVersion { get; set; }

        /// <summary>
        /// The funding status (i.e. published).
        /// </summary>
        [EnumDataType(typeof(FundingStatus))]
        public FundingStatus Status { get; set; }

        /// <summary>
        /// The funding stream the funding relates to.
        /// </summary>
        public Stream Stream { get; set; }

        /// <summary>
        /// The funding period the funding relates to.
        /// </summary>
        public Period Period { get; set; }

        /// <summary>
        /// The grouped organisation (e.g. if we are grouping by LA, the organisation may be Camden).
        /// </summary>
        public GroupingOrganisation GroupingOrganisation { get; set; }

        /// <summary>
        /// Funding value (if it is paid at this level - null if not). QUESTION is my assumption in brackets true?
        /// </summary>
        public FundingValue FundingValue { get; set; }

        /// <summary>
        /// The fundings (child organisation level lines, e.g. providers under an LA) that are grouped into this funding group.
        /// </summary>
        public IEnumerable<Funding> Fundings { get; set; }

        /// <summary>
        /// Does the grouping reflect how the money is paid ('Payment') or is it just useful to show it this way? ('Informational'). 
        /// QUESTION - is GroupingType better?
        /// </summary>
        [EnumDataType(typeof(GroupingReason))]
        public GroupingReason GroupingReason { get; set; }

        /// <summary>
        /// The date the funding was published by a business user. (QUESTION naming - Ryan + Jaspal thought this was best).
        /// </summary>
        public DateTimeOffset StatusChangedDate { get; set; }

        /// <summary>
        /// Date and time when the allocation can be published externally.
        /// </summary>
        public DateTimeOffset ExternalPublicationDate { get; set; }

        /// <summary>
        /// The date the payment will be made to the provider.
        /// </summary>
        public DateTimeOffset? PaymentDate { get; set; }
    }
}