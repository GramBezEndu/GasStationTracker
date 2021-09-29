using System;
using System.Collections.Generic;
using System.Text;

namespace GasStationTracker
{
    public class ValueRecord<T>
    {
        public DateTime Date { get; set; }
        public string Name { get; set; }
        public T Value { get; set; }

        public override string ToString()
        {
            return String.Format("{0} {1} {2}", Date, Name, Value);
        }
    }
}
