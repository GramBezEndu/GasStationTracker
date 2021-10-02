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
        public GraphViewModel() 
        {
            Title = "No data found";
            Points = new List<DataPoint>()
            {
                new DataPoint(0, 1),
                new DataPoint(2, 3),
            };

            Points2 = new List<DataPoint>()
            {
                new DataPoint(0, 2),
                new DataPoint(2, 5),
            };
        }
        public GraphViewModel(string title, List<DataPoint> points)
        {
            Title = title;
            Points = points;
        }

        public string Title { get; private set; }

        public IList<DataPoint> Points { get; private set; }

        public IList<DataPoint> Points2 { get; private set; }
    }
}
