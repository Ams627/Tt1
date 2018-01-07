using System;

namespace Tt1
{
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
}
