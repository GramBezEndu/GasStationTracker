using GasStationTracker.GameData;
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

        public InGameTime IgtPassed 
        { 
            get => igtPassed; 
            private set
            {
                if (value != igtPassed)
                {
                    igtPassed = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Record FirstRecord { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private float cashEarned = 0.00f;

        private int popularityGained = 0;

        private bool sessionEnded = false;

        private DateTime startTime;

        private TimeSpan sessionTime = new TimeSpan(0);

        private InGameTime igtPassed = new InGameTime() 
        {
            Days = 0,
            Hours = 0,
            Minutes = 0,
        };

        public SessionStatistics() { }

        public void Update()
        {
            if (!sessionEnded)
            {
                UpdateFirstRecord();
                if (FirstRecord == null)
                {
                    return;
                }
                Record lastRecord = GetLastRecord();
                CashEarned = (float)(Convert.ToDouble(lastRecord.GetValue(GameIdentifiers.CashDisplay)) - Convert.ToDouble(FirstRecord.GetValue(GameIdentifiers.CashDisplay)));
                PopularityGained = (int)lastRecord.GetValue(GameIdentifiers.PopularityDisplay) - (int)FirstRecord.GetValue(GameIdentifiers.PopularityDisplay);
                SessionTime = DateTime.Now - StartTime;
                TimeSpan inGameTimePassed = new TimeSpan(lastRecord.IGT.Days, lastRecord.IGT.Hours, lastRecord.IGT.Minutes, lastRecord.IGT.Seconds) -
                    new TimeSpan(FirstRecord.IGT.Days, FirstRecord.IGT.Hours, FirstRecord.IGT.Minutes, FirstRecord.IGT.Seconds);
                IgtPassed = new InGameTime()
                {
                    Days = inGameTimePassed.Days,
                    Hours = inGameTimePassed.Hours,
                    Minutes = inGameTimePassed.Minutes,
                };
            }
        }

        public void EndSession()
        {
            sessionEnded = true;
        }

        private void UpdateFirstRecord()
        {
            if (Records != null)
            {
                FirstRecord = Records.FirstOrDefault(x => x.Date >= StartTime);
            }
        }

        private Record GetLastRecord()
        {
            return Records.OrderBy(x => x.Date).Last();
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
