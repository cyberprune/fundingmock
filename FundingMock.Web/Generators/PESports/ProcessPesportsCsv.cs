using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Sfa.Sfs.Mock.Generators
{
    /// <summary>
    /// Process a CSV file containing PE + Sports data.
    /// </summary>
    public class ProcessPesportsCsv
    {
        /// <summary>
        /// Get organisations or organisaation groups from a CSV file.
        /// </summary>
        /// <param name="csvFileName">The filename to lookup from (relates to a sheet in the PE + Sports public spreadsheet).</param>
        /// <param name="groupByLa">Wether to group by LA or not.</param>
        /// <returns>A list or organisation groups.</returns>
        public List<OrgGroup> GetOrgsOrOrgGroups(string csvFileName, bool groupByLa)
        {
            var values = GetDataFromCsv(csvFileName);
            var returnList = new List<OrgGroup>();

            if (groupByLa)
            {
                var laGroups = values.GroupBy(file => file.LaNo);

                foreach (var laGroup in laGroups)
                {
                    var la = new OrgGroup()
                    {
                        Code = laGroup.First().LaNo,
                        Name = laGroup.First().LaName,
                        AprilTotal = laGroup.Sum(x => x.AprilPayment),
                        OctoberTotal = laGroup.Sum(x => x.OctoberPayment),
                        TotalAllocation = laGroup.Sum(x => x.TotalAllocation),
                        Type = csvFileName.Replace(".csv", string.Empty)
                    };

                    foreach (var item in laGroup)
                    {
                        var laProvider = new Provider()
                        {
                            LaEstablishmentNo = item.LaEstablishmentNo,
                            Name = item.SchoolName,
                            OctoberPayment = item.OctoberPayment,
                            AprilPayment = item.AprilPayment,
                            EligiblePupilsCount = item.EligiblePupilsCount,
                            TotalAllocation = item.TotalAllocation
                        };

                        la.Providers.Add(laProvider);
                    }

                    returnList.Add(la);
                }

                return returnList;
            }

            foreach (var value in values)
            {
                var orgGroup = new OrgGroup()
                {
                    Code = value.LaEstablishmentNo,
                    Name = value.SchoolName,
                    AprilTotal = value.AprilPayment,
                    OctoberTotal = value.OctoberPayment,
                    TotalAllocation = value.TotalAllocation,
                    Type = csvFileName.Replace(".csv", string.Empty),
                    Providers = new List<Provider>
                    {
                        new Provider
                        {
                            LaEstablishmentNo = value.LaEstablishmentNo,
                            Name = value.SchoolName,
                            OctoberPayment = value.OctoberPayment,
                            AprilPayment = value.AprilPayment,
                            EligiblePupilsCount = value.EligiblePupilsCount,
                            TotalAllocation = value.TotalAllocation
                        }
                    }
                };

                returnList.Add(orgGroup);
            }

            return returnList;
        }

        /// <summary>
        /// Get the data out of a CSV file.
        /// </summary>
        /// <param name="fileName">The filename of the CSV to read from.</param>
        /// <returns>A list of line values.</returns>
        private List<LineValue> GetDataFromCsv(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var csvStreamPath = assembly.GetManifestResourceNames().Single(str => str.EndsWith("." + fileName));

            var lines = new List<LineValue>();
            var lineNumber = 0;

            using (var stream = assembly.GetManifestResourceStream(csvStreamPath))
            {
                using (var reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();

                        if (lineNumber++ == 0)
                        {
                            continue;
                        }

                        var myLine = new LineValue().FromCsvLineToLineValue(line);

                        if (myLine == null)
                        {
                            continue;
                        }

                        lines.Add(myLine);
                    }
                }
            }

            return lines;
        }
    }

    /// <summary>
    /// PE and Sports spreadsheet data about an org / org group.
    /// </summary>
    public class OrgGroup
    {
        /// <summary>
        /// Default constructor for an org group, setting some default properties.
        /// </summary>
        public OrgGroup()
        {
            Providers = new List<Provider>();
        }

        /// <summary>
        /// The LA number in most instances.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// The name of the LA / provider.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The total for the April payment.
        /// </summary>
        public int AprilTotal { get; set; }

        /// <summary>
        /// The total for the October payment.
        /// </summary>
        public int OctoberTotal { get; set; }

        /// <summary>
        /// The total for the academic year.
        /// </summary>
        public int TotalAllocation { get; set; }

        /// <summary>
        /// The providers who make up art of this org or org group.
        /// </summary>
        public List<Provider> Providers { get; set; }

        /// <summary>
        /// The type of or group (e.g. MaintainedSchools).
        /// </summary>
        public string Type { get; set; }
    }

    /// <summary>
    /// PE and Sports spreadhsset info about a provider.
    /// </summary>
    public class Provider
    {
        /// <summary>
        /// The LA establisment number.
        /// </summary>
        public string LaEstablishmentNo { get; set; }

        /// <summary>
        /// The name of the provider.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Number of eligbile pupils for this grant.
        /// </summary>
        public int EligiblePupilsCount { get; set; }

        /// <summary>
        /// The total allocation for this provider, in pounds.
        /// </summary>
        public int TotalAllocation { get; set; }

        /// <summary>
        /// The october payment, in pounds.
        /// </summary>
        public int OctoberPayment { get; set; }

        /// <summary>
        /// The april payment, in pounds.
        /// </summary>
        public int AprilPayment { get; set; }
    }

    /// <summary>
    /// A line in the PE + sports spreadsheet.
    /// </summary>
    public class LineValue
    {
        /// <summary>
        /// The providers parent LA number.
        /// </summary>
        public string LaNo { get; set; }

        /// <summary>
        /// The name of the LA>
        /// </summary>
        public string LaName { get; set; }

        /// <summary>
        /// The LAs establisment number.
        /// </summary>
        public string LaEstablishmentNo { get; set; }

        /// <summary>
        /// The name of the school.
        /// </summary>
        public string SchoolName { get; set; }

        /// <summary>
        /// Number of eligbile pupils for this grant.
        /// </summary>
        public int EligiblePupilsCount { get; set; }

        /// <summary>
        /// The total allocation for this provider, in pounds.
        /// </summary>
        public int TotalAllocation { get; set; }

        /// <summary>
        /// The october payment, in pounds.
        /// </summary>
        public int OctoberPayment { get; set; }

        /// <summary>
        /// The april payment, in pounds.
        /// </summary>
        public int AprilPayment { get; set; }

        /// <summary>
        /// Convert a line in a CSV file as a string into its component parts. 
        /// </summary>
        /// <param name="csvLine"></param>
        /// <returns></returns>
        public LineValue FromCsvLineToLineValue(string csvLine)
        {
            var values = csvLine.Split(',');

            if (values.Length != 8)
            {
                return null;
            }

            return new LineValue()
            {
                LaNo = values[0],
                LaName = values[1],
                LaEstablishmentNo = values[2],
                SchoolName = values[3],
                EligiblePupilsCount = Convert.ToInt32(values[4]),
                TotalAllocation = Convert.ToInt32(values[5]),
                OctoberPayment = Convert.ToInt32(values[6]),
                AprilPayment = Convert.ToInt32(values[7])
            };
        }
    }
}