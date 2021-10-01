using OxyPlot;
using OxyPlot.Axes;
using System;
using System.Collections.Generic;
using System.Text;

namespace GasStationTracker
{
    public class GraphViewModel
    {
        public GraphViewModel()
        {
            this.Title = "Example";
            this.Points = new List<DataPoint>
                              {
                                  new DataPoint(DateTimeAxis.ToDouble(new DateTime(1970, 10, 10)), 40),
                                  new DataPoint(DateTimeAxis.ToDouble(new DateTime(1970, 10, 11)), 50),
                                  new DataPoint(DateTimeAxis.ToDouble(new DateTime(1970, 10, 12)), 60),
                                  new DataPoint(DateTimeAxis.ToDouble(new DateTime(1970, 10, 13)), 70),
                                  new DataPoint(DateTimeAxis.ToDouble(new DateTime(1970, 10, 14)), 70),
                                  new DataPoint(DateTimeAxis.ToDouble(new DateTime(1970, 10, 15)), 70),
                                  new DataPoint(DateTimeAxis.ToDouble(new DateTime(1970, 10, 16)), 80),
                                  new DataPoint(DateTimeAxis.ToDouble(new DateTime(1970, 10, 17)), 92),
                              };
        }

        public string Title { get; private set; }

        public IList<DataPoint> Points { get; private set; }
    }
}
