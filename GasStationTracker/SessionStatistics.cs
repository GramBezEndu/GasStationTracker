using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace GasStationTracker
{
    public class SessionStatistics : INotifyPropertyChanged
    {
        public RecordCollection Records { get; set; }

        public DateTime StartTime { get; set; }

        public float CashEarned 
        {
            get => cashEarned; 
            private set
            {
                if (value != cashEarned)
                {
                    cashEarned = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int PopularityGained 
        { 
            get => popularityGained;
            private set
            {
                if (value != popularityGained)
                {
                    popularityGained = value;
                    NotifyPropertyChanged();
                }
            }    
        }

        public TimeSpan SessionTime
        {
            get
            {
                return DateTime.Now - StartTime;
            }
        }

        public Record FirstRecord
        {
            get => firstRecord;
            private set => firstRecord = value;
        }

        private Record firstRecord;
        private float cashEarned;
        private int popularityGained;

        public event PropertyChangedEventHandler PropertyChanged;

        public SessionStatistics() { }


        private void UpdateFirstRecord()
        {
            if (Records != null)
            {
                firstRecord = Records.FirstOrDefault(x => x.Date >= StartTime);
            }
        }

        private Record GetLastRecord()
        {
            return Records.OrderBy(x => x.Date).Last();
        }

        public void Update()
        {
            UpdateFirstRecord();
            if (FirstRecord == null)
            {
                return;
            }
            CashEarned = (float)(Convert.ToDouble(GetLastRecord().GetValue(MainWindow.CashDisplay)) - Convert.ToDouble(FirstRecord.GetValue(MainWindow.CashDisplay)));
            PopularityGained = (int)GetLastRecord().GetValue(MainWindow.PopularityDisplay) - (int)FirstRecord.GetValue(MainWindow.PopularityDisplay);
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
