using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FundingMock.Web.Samples
{
    public class ProcessFile
    {
        public List<OrgGroup> GetOrgGroups(string csvFileName, bool groupByLa)
        {
            var values = GetDataFromFile(csvFileName);
            var returnList = new List<OrgGroup>();

            if (groupByLa)
            {
                var laGroup = values.GroupBy(file => file.LaNo);

                foreach (var fileGroup in laGroup)
                {
                    var la = new OrgGroup()
                    {
                        Code = fileGroup.First().LaNo,
                        Name = fileGroup.First().LaName,
                        AprilTotal = fileGroup.Sum(x => x.AprilPayment),
                        OctoberTotal = fileGroup.Sum(x => x.OctoberPayment),
                        TotalAllocation = fileGroup.Sum(x => x.TotalAllocation),
                        Type = csvFileName.Replace(".csv", string.Empty)
                    };

                    foreach (var item in fileGroup)
                    {
                        var laProvider = new Provider()
                        {
                            LaEstablishmentNo = item.LaEstablishmentNo,
                            SchoolName = item.SchoolName,
                            OctoberPayment = item.OctoberPayment,
                            AprilPayment = item.AprilPayment,
                            EligiblePupils = item.EligiblePupils,
                            TotalAllocation = item.TotalAllocation
                        };

                        la.Providers.Add(laProvider);
                    }

                    returnList.Add(la);
                }
            }
            else
            {
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
                                SchoolName = value.SchoolName,
                                OctoberPayment = value.OctoberPayment,
                                AprilPayment = value.AprilPayment,
                                EligiblePupils = value.EligiblePupils,
                                TotalAllocation = value.TotalAllocation
                            }
                        }
                    };

                    returnList.Add(orgGroup);
                }
            }


            return returnList;
        }

        private List<LineValue> GetDataFromFile(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var emailBodyPath = Assembly.GetExecutingAssembly().GetManifestResourceNames().Single(str => str.EndsWith("." + fileName));

            var result = new List<LineValue>();
            var cnt = 0;

            using (var stream = assembly.GetManifestResourceStream(emailBodyPath))
            {
                using (var reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();

                        if (cnt != 0)
                        {
                            var lv = new LineValue();
                            var myLine = lv.FromCsvLineToLineValue(line);

                            if (myLine != null)
                            {
                                result.Add(myLine);
                            }
                        }

                        cnt += 1;
                    }
                }
            }

            return result;
        }

    }

    public class OrgGroup
    {
        public OrgGroup()
        {
            Providers = new List<Provider>();
        }

        public string Code { get; set; }
        public string Name { get; set; }

        public int AprilTotal { get; set; }
        public int OctoberTotal { get; set; }
        public int TotalAllocation { get; set; }

        public List<Provider> Providers { get; set; }

        public string Type { get; set; }
    }

    public class Provider
    {
        public string LaEstablishmentNo { get; set; }
        public string SchoolName { get; set; }
        public int EligiblePupils { get; set; }
        public int TotalAllocation { get; set; }
        public int OctoberPayment { get; set; }
        public int AprilPayment { get; set; }

    }
   public class LineValue
    {
        public  string LaNo { get; set; }
        public  string LaName { get; set; }
        public  string LaEstablishmentNo { get; set; }
        public  string SchoolName { get; set; }
        public  int EligiblePupils { get; set; }
        public  int TotalAllocation { get; set; }
        public  int OctoberPayment { get; set; }
        public  int AprilPayment { get; set; }

        public LineValue FromCsvLineToLineValue(string csvLine)
        {
            LineValue line = null;
            var values = csvLine.Split(',');

            if (values.Length == 8)
            {
                line = new LineValue()
                {
                    LaNo = values[0],
                    LaName = values[1],
                    LaEstablishmentNo = values[2],
                    SchoolName = values[3],
                    EligiblePupils = Convert.ToInt32(values[4]),
                    TotalAllocation = Convert.ToInt32(values[5]),
                    OctoberPayment = Convert.ToInt32(values[6]),
                    AprilPayment = Convert.ToInt32(values[7])
                };
            }

            return line;
        }
    }
}