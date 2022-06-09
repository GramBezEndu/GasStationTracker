namespace GasStationTracker
{
    using System.Collections.Generic;
    using OxyPlot;

    public class GraphViewModel
    {
        public Dictionary<string, IList<DataPoint>> Points { get; private set; }

        public PlotModel Plot { get; set; } = new PlotModel() { Title = "No data found" };
    }
}
