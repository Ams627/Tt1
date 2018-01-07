using System;
using System.Globalization;
using System.Linq;

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
            DateTime.TryParseExact("20" + s.Substring(offset, 6), "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate);
            DateTime.TryParseExact("20" + s.Substring(offset + 6, 6), "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate);
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
}
