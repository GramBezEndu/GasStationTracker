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

        public DateTime StartTime
        {
            get => startTime;
            set
            {
                if (startTime != value)
                {
                    startTime = value;
                    sessionEnded = false;
                }
            }
        }

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
            get => sessionTime;
            private set
            {
                if (value != sessionTime)
                {
                    sessionTime = value;
                    NotifyPropertyChanged();
                }
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
        private bool sessionEnded = false;
        private DateTime startTime;
        private TimeSpan sessionTime;

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
            if (!sessionEnded)
            {
                UpdateFirstRecord();
                if (FirstRecord == null)
                {
                    return;
                }
                CashEarned = (float)(Convert.ToDouble(GetLastRecord().GetValue(MainWindow.CashDisplay)) - Convert.ToDouble(FirstRecord.GetValue(MainWindow.CashDisplay)));
                PopularityGained = (int)GetLastRecord().GetValue(MainWindow.PopularityDisplay) - (int)FirstRecord.GetValue(MainWindow.PopularityDisplay);
                SessionTime = DateTime.Now - StartTime;
            }
        }

        public void EndSession()
        {
            sessionEnded = true;
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
