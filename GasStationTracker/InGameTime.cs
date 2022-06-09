namespace GasStationTracker
{
    using System;

    public class InGameTime
    {
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

        public int Days { get; set; }

        public int Hours { get; set; }

        public int Minutes { get; set; }

        public int Seconds { get; set; }

        public override string ToString()
        {
            return string.Format("{0}:{1}:{2}", Days, Hours, Minutes);
        }
    }
}