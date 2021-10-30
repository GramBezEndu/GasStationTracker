using System;
using System.Collections.Generic;
using System.Text;

namespace GasStationTracker
{
    public class Record
    {
        public DateTime Date { get; set; }

        public InGameTime IGT { get; set; }

        public List<SingleValue> SingleRecords { get; set; } = new List<SingleValue>();

        public Record()
        {

        }

        public object GetValue(string name)
        {
            return SingleRecords.Find(s => s.Name.Equals(name)).Value;
        }
    }
}
