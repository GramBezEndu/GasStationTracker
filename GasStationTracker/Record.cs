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

        public T GetValue<T>(string name)
        {
            T result;
            SingleValue<T> setting = (SingleValue<T>)Convert.ChangeType(
                                 SingleRecords.Find(s => s.Name.Equals(name)), typeof(SingleValue<T>), null);
            result = setting.Value;
            return result;
        }
    }
}
