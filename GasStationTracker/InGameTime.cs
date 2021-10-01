namespace GasStationTracker
{
    public class InGameTime
    {
        public int Days { get; set; }
        public int Hours { get; set; }
        public int Seconds { get; set; }

        public override string ToString()
        {
            return string.Format("{0}:{1}:{2}", Days, Hours, Seconds);
        }
    }
}