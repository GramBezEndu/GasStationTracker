using System;
using System.Collections.Generic;
using System.Text;

namespace GasStationTracker
{
    public class SingleValue
    {
        public string Name { get; set; }

        public virtual Type Type { get; }
    }

    public class SingleValue<T> : SingleValue
    {
        public T Value { get; set; }

        public override Type Type { get { return typeof(T); } }

        public override string ToString()
        {
            return String.Format("{0} {1}", Name, Value);
        }
    }
}
