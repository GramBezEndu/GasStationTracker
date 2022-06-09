namespace GasStationTracker
{
    using System;
    using System.Linq;
    using OxyPlot;
    using OxyPlot.Axes;
    using OxyPlot.Series;

    public static class PlotModelExtensions
    {
        public static void UpdateGraphLineSeries(this PlotModel plot, string currentFilter)
        {
            ElementCollection<Series> series = plot.Series;
            foreach (Series serie in series)
            {
                serie.IsVisible = false;
            }

            if (currentFilter != string.Empty)
            {
                Series currentSeries = series.FirstOrDefault(x => x.Title == currentFilter);
                if (currentSeries != null)
                {
                    if (currentSeries.IsVisible == false)
                    {
                        currentSeries.IsVisible = true;
                        AutoScaleGraph(plot, currentSeries as LineSeries);
                    }
                }
            }

            plot.InvalidatePlot(true);
        }

        public static void AutoScaleGraph(this PlotModel model)
        {
            LineSeries firstSeries = (LineSeries)model.Series.FirstOrDefault();
            if (firstSeries != null)
            {
                AutoScaleGraph(model, firstSeries);
            }
        }

        private static void AutoScaleGraph(this PlotModel model, LineSeries currentSeries)
        {
            IOrderedEnumerable<DataPoint> points = currentSeries.Points.OrderBy(x => x.X);
            if (points.Count() >= 1 && model.Axes.Count() >= 1)
            {
                model.Axes[0].Reset();
                model.Axes[0].Minimum = DateTimeAxis.ToDouble(points.FirstOrDefault().X);
                model.Axes[0].Maximum = DateTimeAxis.ToDouble(
                    DateTimeAxis.ToDateTime(points.LastOrDefault().X) +
                    new TimeSpan(0, 0, 60));
                model.Axes[1].Reset();
            }
        }
    }
}
