namespace GasStationTracker
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows.Controls;
    using System.Windows.Data;
    using OxyPlot;
    using OxyPlot.Axes;
    using OxyPlot.Series;

    public class RecordCollection : ObservableCollection<Record>
    {
        private DataGrid dataGridBinding;

        public RecordCollection()
        {
        }

        public RecordCollection(DataGrid binding, PlotModel plot)
            : base()
        {
            DataGridBinding = binding;
            Plot = plot;
        }

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

        protected override void InsertItem(int index, Record item)
        {
            if (DataGridBinding != null)
            {
                PrepareDataGrid(item);
            }

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

                    Series series = Plot.Series.First(x => x.Title == singleValue.Name);
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
                            Header = val.Name,
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
