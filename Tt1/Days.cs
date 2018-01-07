using System;

namespace Tt1
{
    class Days
    {
        [Flags] enum DayFlags
        {
            Monday = 128,
            Tuesday = 64,
            Wednesday = 32,
            Thursday = 16, 
            Friday = 8,
            Saturday = 4,
            Sunday = 2,
            BankHoliday = 1
        }
        private DayFlags _days;
        public Days(string s, int offset)
        {
            if (s.Length - offset <= 8)
            {
                throw new ArgumentException("Length of days string supplied for BS record minus the supplied offset must be more than 8");
            }
            UInt32 uintflags = 0;
            for (int i = 0; i < 7; i++)
            {
                if (s[i + offset] == '1')
                {
                    uintflags = (uintflags << 1) | 1;
                }
                else if (s[i + offset] == '0')
                {
                    uintflags = (uintflags << 1);
                }
                else
                {
                    throw new Exception($"{s[i + offset]} is an invalid character in the days string - should be '0' or '1'");
                }
            }
            if (s[offset + 7] == ' ')
            {
                uintflags = (uintflags << 1);
            }
            else if (s[offset + 7] == 'X')
            {
                uintflags = (uintflags << 1) | 1;
            }
            else
            {
                throw new Exception($"{s[offset + 7]} is an invalid character in the days string - should be '0' or '1'");
            }
            _days = (DayFlags)uintflags;
        }
        public UInt32 GetBitField => (UInt32) _days;
    }
}
