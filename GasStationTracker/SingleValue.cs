using System;
using System.Collections.Generic;
using System.Text;

namespace GasStationTracker
{
    public class SingleValue
    {
        public string Name { get; set; }

        public Type Type { get { return Value.GetType(); } }

        public object Value { get; set; }
    }
}
