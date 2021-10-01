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

namespace GasStationTracker
{
    public class RecordCollection : ObservableCollection<Record>
    {
        public DataGrid Binding { get; private set; }

        public RecordCollection() { }

        public RecordCollection(DataGrid binding)
            : base()
        {
            Binding = binding;
        }

        protected override void InsertItem(int index, Record item)
        {
            if (Binding != null)
                PrepareDataGrid(item);
            base.InsertItem(index, item);
        }

        private void PrepareDataGrid(Record item)
        {
            for (int i = 0; i < item.SingleRecords.Count; i++)
            {
                SingleValue val = item.SingleRecords[i];
                if (val != null)
                {
                    if (!Binding.Columns.Any(x => (string)x.Header == val.Name))
                    {
                        DataGridTextColumn textColumn = new DataGridTextColumn();
                        textColumn.Header = val.Name;
                        Binding newBinding = new Binding("SingleRecords[" + i + "].Value");
                        //newBinding.Path = new PropertyPath("Name");
                        textColumn.Binding = newBinding;
                        Binding.Columns.Add(textColumn);
                    }
                }
            }
        }
    }
}
