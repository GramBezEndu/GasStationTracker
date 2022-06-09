namespace GasStationTracker
{
    using System;

    public class SingleValue
    {
        public string Name { get; set; }

        public Type Type => Value.GetType();

        public object Value { get; set; }
    }
}
