using System;
using System.Collections.Generic;
using System.Linq;

namespace Tt1
{
    class TimetableTime
    {
        public class IntermediateTimes
        {
            public TimetableTime Arrive { get; private set; }
            public TimetableTime Depart { get; private set; }
            public TimetableTime Pass { get; private set; }
            public TimetableTime PublicArrive { get; private set; }
            public TimetableTime PublicDepart { get; private set; }
            public IntermediateTimes(string line, int offset)
            {
                if (line.Length - offset < 23)
                {
                    throw new Exception($"Cannot construct a {this.GetType().ToString()} as the string supplied (length {line.Length}) minus the offset ({offset}) is not long enough (must be at least 23).");
                }
                Arrive = new TimetableTime(line, offset);
                Depart = new TimetableTime(line, offset + 5);
                Pass = new TimetableTime(line, offset + 10);
                PublicArrive = new TimetableTime(line, offset + 15);
                PublicDepart = new TimetableTime(line, offset + 19);
            }
        }
        /// <summary>
        /// number of minutes from midnight - zero to 1439
        /// </summary>
        private int _minutes;
        public TimetableTime(string line, int offset, bool noThrowOnSpaces = false)
        {
            if (line.Length - offset < 4)
            {
                throw new Exception($"Cannot construct a {this.GetType().ToString()} as the string supplied (length {line.Length}) minus the offset ({offset}) is not long enough.");
            }
            var timePortion = line.Substring(offset, 4);
            bool allSpaces = timePortion.All(char.IsWhiteSpace);
            bool allDigits = timePortion.All(char.IsDigit);
            if (allDigits)
            {
                if (timePortion[0] > '2' || timePortion[2] > '5')
                {
                    throw new Exception($"Invalid time '{timePortion}'");
                }
                _minutes = (timePortion[0] - '0') * 600 + (timePortion[1] - '0') * 60 + (timePortion[2] - '0') * 10 + (timePortion[3] - '0');
                if (_minutes >= 24 * 60)
                {
                    throw new Exception($"Invalid time '{timePortion}'");
                }
            }
            else if (!allSpaces || noThrowOnSpaces)
            {
                throw new Exception($"Invalid time '{timePortion}'");
            }
            if (allSpaces)
            {
                _minutes = -1;
            }
        }

        public static UInt32 TimeDiff(TimetableTime earlier, TimetableTime later)
        {
            UInt32 result;
            if (later._minutes < earlier._minutes)
            {
                result = (UInt32)(later._minutes + 24 * 60 - earlier._minutes);
            }
            else
            {
                result = (UInt32)(later._minutes - earlier._minutes);
            }
            return result;
        }

        public int MinutesSinceMidnight => _minutes;
        public override string ToString() => $"{_minutes / 60:D2}:{_minutes % 60:D2}";
    }
}
