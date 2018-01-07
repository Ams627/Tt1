using System;

namespace Tt1
{
    class TrainActivities
    {
        [Flags]
        enum ActivityFlags
        {
            Pickup = 1,
            Setdown = 2
        }

        private ActivityFlags _trainActivities;
        public TrainActivities(string line, int offset)
        {
            _trainActivities = 0;
            if (line.Length - offset < 12)
            {
                throw new Exception($"Cannot construct a {this.GetType().ToString()} as the string supplied (length {line.Length}) minus the offset ({offset}) is not long enough.");
            }

            for (int i = 0; i < 6; ++i)
            {
                var activity = line.Substring(offset + i * 2, 2).Trim();
                switch (activity)
                {
                    // pick up and set down passengers
                    case "T":
                        _trainActivities |= ActivityFlags.Setdown | ActivityFlags.Pickup;
                        break;
                    // setdown only - passengers are not permitted to board
                    case "D":
                        _trainActivities |= ActivityFlags.Setdown;
                        break;
                    // pickup only - passengers are not permitted to alight
                    case "U":
                        _trainActivities |= ActivityFlags.Pickup;
                        break;
                }
            }
        }

        public bool CanPickup => _trainActivities.HasFlag(ActivityFlags.Pickup);
        public bool CanSetdown => _trainActivities.HasFlag(ActivityFlags.Setdown);
        public bool CanPickupAndSetDown => _trainActivities.HasFlag(ActivityFlags.Pickup) && _trainActivities.HasFlag(ActivityFlags.Setdown);
        public UInt32 GetflagsAsUint => (UInt32)_trainActivities;
    }
}
