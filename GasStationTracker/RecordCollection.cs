using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace GasStationTracker
{
    public class RecordCollection : ObservableCollection<Record>
    {
        public DataGrid DataGridBinding { get; private set; }

        public PlotModel Plot { get; private set; }

        public RecordCollection() { }

        public RecordCollection(DataGrid binding, PlotModel plot)
            : base()
        {
            DataGridBinding = binding;
            Plot = plot;
        }

        protected override void InsertItem(int index, Record item)
        {
            if (DataGridBinding != null)
                PrepareDataGrid(item);
            if (Plot != null)
                UpdateGraphs(item);
            base.InsertItem(index, item);
        }

        private void UpdateGraphs(Record record)
        {
            for (int i = 0; i < record.SingleRecords.Count; i++)
            {
                SingleValue singleValue = record.SingleRecords[i];
                if (singleValue != null && singleValue.Name == "Cash")
                {
                    if (!Plot.Series.Any(x => x.Title == singleValue.Name))
                    {
                        Plot.Series.Add(new LineSeries() 
                        {
                            Title = singleValue.Name 
                        });
                    }
                    var series = Plot.Series.First(x => x.Title == singleValue.Name);
                    (series as LineSeries).Points.Add(new DataPoint(DateTimeAxis.ToDouble(record.Date), Convert.ToDouble(singleValue.Value)));
                }
            }
        }

        private void PrepareDataGrid(Record item)
        {
            for (int i = 0; i < item.SingleRecords.Count; i++)
            {
                SingleValue val = item.SingleRecords[i];
                if (val != null)
                {
                    if (!DataGridBinding.Columns.Any(x => (string)x.Header == val.Name))
                    {
                        DataGridTextColumn textColumn = new DataGridTextColumn();
                        textColumn.Header = val.Name;
                        Binding newBinding = new Binding("SingleRecords[" + i + "].Value");
                        textColumn.Binding = newBinding;
                        DataGridBinding.Columns.Add(textColumn);
                    }
                }
            }
        }
    }
}
