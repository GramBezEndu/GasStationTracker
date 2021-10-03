using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Text;

namespace GasStationTracker
{
    public class GraphViewModel
    {
        public Dictionary<string, IList<DataPoint>> Points { get; private set; }

        public PlotModel Plot { get; set; } = new PlotModel() { Title = "No data found" };
    }
}
