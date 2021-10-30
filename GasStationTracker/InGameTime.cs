using System;

namespace GasStationTracker
{
    public class InGameTime
    {
        public int Days { get; set; }

        public int Hours { get; set; }

        public int Minutes { get; set; }

        public int Seconds { get; set; }

        public InGameTime()
        {

        }

        public InGameTime(float time)
        {
            TimeSpan timeSpan = TimeSpan.FromMinutes(time);
            Days = timeSpan.Days;
            Hours = timeSpan.Hours;
            Minutes = timeSpan.Minutes;
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}:{2}", Days, Hours, Minutes);
        }
    }
}