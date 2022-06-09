namespace GasStationTracker
{
    using System;
    using System.Collections.Generic;

    public class Record
    {
        public Record()
        {
        }

        public DateTime Date { get; set; }

        public InGameTime IGT { get; set; }

        public List<SingleValue> SingleRecords { get; set; } = new List<SingleValue>();

        public object GetValue(string name)
        {
            return SingleRecords.Find(s => s.Name.Equals(name)).Value;
        }
    }
}
