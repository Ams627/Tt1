using System;

namespace Tt1
{
    /// <summary>
    /// Train UID - a digit, an alphanumeric, then finally two digits.
    /// </summary>
    class TrainUID
    {
        UInt32 _trainUid;
        public TrainUID(string line, int offset)
        {
            if (line.Length - offset < 4)
            {
                throw new Exception($"Cannot construct a {this.GetType().ToString()} as the string supplied (length {line.Length}) minus the offset ({offset}) is not long enough.");
            }

            if (!char.IsDigit(line[offset]) || !char.IsLetterOrDigit(line[offset + 1]) || !char.IsDigit(line[offset + 2]) || !char.IsDigit(line[offset + 3]))
            {
                throw new Exception($"Invalid train UID: '{line.Substring(offset, 4)}'");
            }
            var char1 = line[offset + 1] - '0';
            if (char1 > 9)
            {
                char1 += '0';
                char1 -= 'A';
            }
            int i = (line[offset] - '0') * 100 + (line[offset + 2] - '0') * 10 + (line[offset + 3] - '0') + (char1 * 1000);
            _trainUid = (UInt32)i;
        }

        public override string ToString()
        {
            var alphanumeric = _trainUid / 1000;
            char c = (char)(alphanumeric > 9 ? alphanumeric + 'A' : alphanumeric + '0');
            var s = (_trainUid % 1000).ToString("D3");

            return "" + s[0] + c + s[1] + s[2];
        }
    }
}
