using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tt1
{
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
                var fileManager = new OutputFileManager("s:\\out");
                var oneRun = new List<string>();
                var crsCompressor = new CrsCodec();
                var outfileCompressed = "s:\\amstt.comp";
                bool firstBS = true;
                UInt32 maxdiff = 0;
                var minDate = DateTime.Today;
                var maxDate = DateTime.Today;
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

                                var lastYearEpoch = new DateTime(1, 1, DateTime.Today.Year - 1);
                                var daysRunsFrom = bs.RunsFrom - lastYearEpoch;
                                var daysRunsTo = bs.RunsTo - lastYearEpoch;
                                var days = new Days(line, 21);
                                var uid2 = new TrainUID(line, 32);
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
                                            if (recordType == "LI")
                                            {
                                                var times = new TimetableTime.IntermediateTimes(line, 10);
                                                var activities = new TrainActivities(line, 42);
                                                if (activities.CanPickupAndSetDown)
                                                {
                                                    var diff = TimetableTime.TimeDiff(times.PublicArrive, times.PublicDepart);
                                                    Console.WriteLine($"diff:{diff}");
                                                    Console.WriteLine($"depart:{times.PublicDepart}");
                                                }
                                            }
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

                Console.WriteLine($"max diff is {maxdiff}");
                Console.WriteLine($"min date is {minDate:yyyy-MM-dd}");
                Console.WriteLine($"max date is {maxDate:yyyy-MM-dd}");

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
                Console.WriteLine();
            }

        }
    }
}
