using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tt1
{
    static class TTUtils
    {
        public static char GetOneOf(string input, int offset, string options, string errorMessage)
        {
            bool success = options.Contains(input[offset]);
            if (!success)
            {
                throw new Exception($"{errorMessage}: expected one of {string.Join(",", options)}");
            }
            return input[offset];
        }

        public static (DateTime, DateTime) GetStartEndDate(string s, int offset)
        {
            if (s.Length - offset < 12)
            {
                throw new Exception("string not long enough.");
            }
            DateTime.TryParseExact(s.Substring(offset, 6), "yyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate);
            DateTime.TryParseExact(s.Substring(offset + 6, 6), "yyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate);
            return (startDate, endDate);
        }
    }
    class BSRecord
    {
        public char TransactionType { get; set; }
        public string TrainUID { get; set; }
        public DateTime RunsFrom { get; set; }
        public DateTime RunsTo { get; set; }
        public Days Days { get; set; }
        public char TrainStatus { get; set; }
        public string TrainCategory { get; set; }
        public string TrainIdentity { get; set; }
        public string HeadCode { get; set; }
        public char CourseIndicator { get; set; }
        public string ProfitCentre { get; set; }
        public char BusinessSector { get; set; }
        public string PowerType { get; set; }
        public string TimingLoad { get; set; }
        public string Speed{ get; set; }
        public string OperatingChars { get; set; }
        public char TrainClass { get; set; }
        public char Sleepers { get; set; }
        public char Reservations { get; set; }
        public char ConnectIndicator { get; set; }
        public string CateringCode { get; set; }
        public string ServiceBranding { get; set; }
        public char Spare { get; set; }
        public char StpIndicator { get; set; }

        public BSRecord(string s)
        {
            if (s.Substring(0, 2) != "BS")
            {
                throw new Exception("BS expected");
            }
            TransactionType = TTUtils.GetOneOf(s, 2, "NDR", "Invalid Transaction Type");
            TrainUID = s.Substring(3, 6);
            (RunsFrom, RunsTo) = TTUtils.GetStartEndDate(s, 9);
            Days = new Days(s, 21);
            TrainStatus = s[29];
            TrainCategory = s.Substring(30, 2);
            TrainIdentity = s.Substring(32, 4);
            HeadCode = s.Substring(36, 4);
            CourseIndicator = s[40];
            ProfitCentre = s.Substring(41, 8);
            BusinessSector = s[49];
            PowerType = s.Substring(50, 3);
            TimingLoad = s.Substring(53, 4);
            Speed = s.Substring(57, 3);
            OperatingChars = s.Substring(60, 6);
            TrainClass = s[66];
            Sleepers = s[67];
            Reservations = s[68];
            ConnectIndicator = s[69];
            CateringCode = s.Substring(70, 4);
            ServiceBranding = s.Substring(74, 4);
            Spare = s[78];
            StpIndicator = s[79];
        }
    }
    class Days
    {
        private bool[] _days = new bool[8];
        public Days(string s, int offset)
        {
            if (s.Length - offset <= 8)
            {
                throw new ArgumentException("Length of days string supplied for BS record minus the supplied offset must be more than 8");
            }
            for (int i = 0; i < 7; i++)
            {
                if (s[i + offset] == '1')
                {
                    _days[i] = true;
                }
                else if (s[i + offset] == '0')
                {
                    _days[i] = false;
                }
                else
                {
                    throw new Exception($"{s[i + offset]} is an invalid character in the days string - should be '0' or '1'");
                }
            }
            if (s[offset + 7] == ' ')
            {
                _days[7] = false;
            }
            else if (s[offset + 7] == 'X')
            {
                _days[7] = true;
            }
            else
            {
                throw new Exception($"{s[offset + 7]} is an invalid character in the days string - should be '0' or '1'");
            }
        }
    }
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                var tipLocToCrs = new Dictionary<string, string>();
                var startTime = DateTime.Now;
                var filename = "s:\\ttisf772.mca";
                var count = 0;
                var linenumber = 0;
                bool validRecord = false;
                string currentBSRecord;

                foreach (var line in File.ReadLines(filename))
                {
                    try
                    {
                        if (line.Length > 2 && line.Substring(0, 2) == "TI")
                        {
                            var tiploc = line.Substring(2, 7);
                            var crs = line.Substring(53, 3);
                            tipLocToCrs.Add(tiploc, crs);
                        }
                        else if (line.Substring(0, 2) == "BS")
                        {
                            var bs = new BSRecord(line);
                            if (bs.RunsTo > DateTime.Today)
                            {
                                validRecord = true;
                                currentBSRecord = line;
                                count++;
                            }
                        }
                        if (validRecord)
                        {
                            if (line.Substring(0, 2) == "LO")
                            {
                            }
                            else if (line.Substring(0, 2) == "LI")
                            {
                            }
                            else if (line.Substring(0, 2) == "CR")
                            {
                            }
                            else if (line.Substring(0, 2) == "LT")
                            {
                                validRecord = false;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error at line {linenumber + 1} : {e.Message}");
                    }
                    linenumber++;
                    if (linenumber % 100000 == 99999)
                    {
                        Console.WriteLine($"{linenumber + 1}");
                    }
                }
                var timetaken = DateTime.Now - startTime;
                Console.WriteLine($"count it {count} - time was {timetaken.TotalSeconds} seconds");
            }
            catch (Exception ex)
            {
                var codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
                var progname = Path.GetFileNameWithoutExtension(codeBase);
                Console.Error.WriteLine(progname + ": Error: " + ex.Message);
            }

        }
    }
}
