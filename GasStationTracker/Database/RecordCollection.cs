using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

namespace GasStationTracker
{
    public class RecordCollection : ObservableCollection<Record>
    {
        private DataGrid dataGridBinding;

        public DataGrid DataGridBinding 
        { 
            get => dataGridBinding;
            private set
            {
                if (dataGridBinding != value)
                {
                    dataGridBinding = value;
                }
            }
        }

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
            {
                UpdateGraphs(item);
                Plot.AutoScaleGraph();
            }
            base.InsertItem(index, item);
        }

        private void UpdateGraphs(Record record)
        {
            for (int i = 0; i < record.SingleRecords.Count; i++)
            {
                SingleValue singleValue = record.SingleRecords[i];
                if (singleValue != null)
                {
                    if (!Plot.Series.Any(x => x.Title == singleValue.Name))
                    {
                        if (singleValue.Name == "Cash")
                        {
                            Plot.Series.Add(new LineSeries()
                            {
                                Title = singleValue.Name,
                                Color = OxyColors.Blue,                             
                            });
                        }
                        else
                        {
                            Plot.Series.Add(new LineSeries()
                            {
                                Title = singleValue.Name,
                                Color = OxyColors.Blue,
                                IsVisible = false,
                            });
                        }
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
                        DataGridTextColumn textColumn = new DataGridTextColumn
                        {
                            Header = val.Name
                        };
                        Binding newBinding = new Binding("SingleRecords[" + i + "].Value");
                        textColumn.Binding = newBinding;
                        DataGridBinding.Columns.Add(textColumn);
                    }
                }
            }
        }
    }
}
