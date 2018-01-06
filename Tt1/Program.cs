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

        public static UInt32 GetMinutes(string s, int offset = 0)
        {
            UInt32 result;
            if (s.Length - offset < 4)
            {
                throw new Exception("string not long enough");
            }
            var timestring = s.Substring(offset, 4);
            if (timestring.All(char.IsWhiteSpace))
            {
                result = 0xFFFFFFFF;
            }
            else if (!timestring.All(char.IsDigit))
            {
                throw new Exception("Invalid character in time");
            }
            else
            {
                result = (UInt32)((timestring[0] - '0') * 600 + (timestring[1] - '0') * 60 + (timestring[2] - '0') * 10 + (timestring[3] - '0'));
            }
            if (result != 0xFFFFFFFF && result > 1439)
            {
                throw new Exception($"invalid time: {s.Substring(offset, 4)}");
            }
            return result;
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

    class CrsCodec
    {
        UInt32 _currentCount = 0;
        private Dictionary<string, UInt32> _crsToInt = new Dictionary<string, UInt32>();
        private Dictionary<UInt32, string> _intToCrs = new Dictionary<UInt32, string>();

        public CrsCodec()
        {
            _currentCount = 0;
        }
        public UInt32 AddCrs(string s)
        {
            UInt32 result;
            if (_crsToInt.TryGetValue(s, out var i))
            {
                result = i;
            }
            result = _currentCount++;
            _crsToInt[s] = result;
            return result;
        }

        public string GetCrs(UInt32 i)
        {
            if (!_intToCrs.TryGetValue(i, out var crs))
            {
                throw new Exception($"the value {i} does not correspond to a known CRS code");
            }
            return crs;
        }
        public UInt32 GetCompressedCrs(string crs)
        {
            if (!_crsToInt.TryGetValue(crs, out var result))
            {
                throw new Exception($"Unknown CRS: {crs}");
            }
            return result;
        }
        public void WriteCrsDictionary(BinaryWriter bw)
        {
            bw.Write((UInt16)_crsToInt.Count());
            foreach(var entry in _crsToInt)
            {
                var crs = entry.Key;
                if (crs.Length != 3)
                {
                    throw new Exception("Fatal error - CRS code must be three characters long");
                }
                UInt16 crsBase26 = (UInt16)((crs[0] - 'A') * 26 * 26 + (crs[1] - 'A') * 26 + (crs[2] - 'A'));
                bw.Write(crsBase26);
            }
        }

        public static CrsCodec CreateFromFile(string filename)
        {
            var codec = new CrsCodec();
            codec.ReadFromFile(filename);
            return codec;
        }

        public static CrsCodec CreateFromFile(BinaryReader br)
        {
            var codec = new CrsCodec();
            codec.ReadFromFile(br);
            return codec;
        }


        public void ReadFromFile(string filename)
        {
            _currentCount = 0;
            using (var stream = new FileStream(filename, FileMode.Open))
            using (var reader = new BinaryReader(stream))
            {
                var length = reader.ReadUInt16();
                for (UInt32 i = 0; i < length; ++i)
                {
                    var crsBase26 = reader.ReadUInt16();
                    string crs = "" + (char)('A' + crsBase26 / 26 / 26) + (char)(('A' + (crsBase26 / 26) % 26)) + (char)('A' + crsBase26 % 26);
                    _crsToInt.Add(crs, i);
                    _intToCrs.Add(i, crs);
                }
            }
        }

        public void ReadFromFile(BinaryReader reader)
        {
            var length = reader.ReadUInt16();
            for (UInt32 i = 0; i < length; ++i)
            {
                var crsBase26 = reader.ReadUInt16();
                string crs = "" + (char)('A' + crsBase26 / 26 / 26) + (char)(('A' + (crsBase26 / 26) % 26)) + (char)('A' + crsBase26 % 26);
                _crsToInt.Add(crs, i);
                _intToCrs.Add(i, crs);
            }
        }
    }

    internal class Program
    {
        static V GetValueOrNull<K, V>(Dictionary<K, V> dict, K key)
        {
            V result = default;
            if (dict.TryGetValue(key, out var value))
            {
                result = value;
            }
            return result;
        }
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
                var currentOrigin = "";
                var fileManager = new OutputFileManager("s:\\out");
                var oneRun = new List<string>();
                var crsCompressor = new CrsCodec();
                var outfileCompressed = "s:\\amstt.comp";
                bool firstBS = true;
                using (var outstream = new FileStream(outfileCompressed, FileMode.Truncate))
                using (var binarywriter = new BinaryWriter(outstream))
                {
                    foreach (var line in File.ReadLines(filename))
                    {
                        try
                        {
                            var recordType = line.Substring(0, 2);
                            if (line.Length > 2 && recordType == "TI")
                            {
                                var tiploc = line.Substring(2, 7);
                                var crs = line.Substring(53, 3);
                                if (crs.All(c => char.IsLetterOrDigit(c)))
                                {
                                    tipLocToCrs.Add(tiploc, crs);
                                    crsCompressor.AddCrs(crs);
                                }
                            }
                            else if (recordType == "BS")
                            {
                                if (firstBS)
                                {
                                    crsCompressor.WriteCrsDictionary(binarywriter);
                                    firstBS = false;
                                }
                                var bs = new BSRecord(line);
                                if (bs.RunsTo > DateTime.Today)
                                {
                                    validRecord = true;
                                    oneRun.Add(line);
                                    count++;
                                }
                            }
                            if (validRecord)
                            {
                                if (recordType == "LO" || recordType == "LI" || recordType == "LT")
                                {
                                    if (!(recordType == "LI" && line.Substring(15, 4).All(char.IsWhiteSpace)))
                                    {
                                        var tiploc = line.Substring(2, 7);
                                        var crs = GetValueOrNull(tipLocToCrs, tiploc);
                                        if (crs != null)
                                        {
                                            var crsIndex = crsCompressor.GetCompressedCrs(crs);
                                            var offset = (recordType == "LI") ? 29 : 15;
                                            var timeToStore = TTUtils.GetMinutes(line, offset);
                                            var compressedWord = (crsIndex << 11) | timeToStore;
                                            if (recordType == "LO")
                                            {
                                                compressedWord |= 0x80000000;
                                            }
                                            binarywriter.Write(compressedWord);
                                        }
                                    }
                                }
                                if (recordType == "LT")
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
                        if (linenumber % 1000 == 999)
                        {
                            Console.WriteLine($"{linenumber + 1}");
                        }
                    }
                }

                var filestream = new FileStream(outfileCompressed, FileMode.Open);
                var br = new BinaryReader(filestream);
                var codec = CrsCodec.CreateFromFile(br);
                while (filestream.Position != filestream.Length)
                {
                    var word = br.ReadUInt32();
                    var crs = codec.GetCrs((word & 0x7FFFFFFF) >> 11);
                    var minutes = word & 0x7FF;
                    var hours = minutes / 60;
                    var remainingMinutes = minutes % 60;
                    Console.WriteLine($"{crs} {hours}:{remainingMinutes}");
                }
                var timetaken = DateTime.Now - startTime;
                Console.WriteLine($"count is {count} - time was {timetaken.TotalSeconds} seconds");
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
