using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Memory;
using Newtonsoft.Json;
using OxyPlot;
using OxyPlot.Axes;

namespace GasStationTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public int intervalMinutes = 0;
        public int intervalSeconds = 10;

        string fileName = "Data.json";

        JsonSerializerSettings settings = new JsonSerializerSettings 
        {
            NullValueHandling = NullValueHandling.Ignore,
            DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
            Formatting = Formatting.None,
            TypeNameHandling = TypeNameHandling.Auto,
        };

        public PlotModel Plot { get; private set; } = new PlotModel();
        public RecordCollection Records { get => records; private set => records = value; }
        private RecordCollection records;
        System.Windows.Threading.DispatcherTimer dispatcherTimer;
        Mem memoryHandler;
        bool isTracking = false;

        #region GssData
        const string processName = "GSS2-Win64-Shipping";
        int gameProcessId;

        public bool IsTracking
        {
            get => isTracking;
            set
            {
                if (isTracking != value)
                {
                    if (value == true)
                    {
                        StartStop.Content = "Stop tracking";
                        Log("Attatched to process " + processName);
                    }
                    else
                    {
                        StartStop.Content = "Start tracking";
                        Log("Closed process " + processName);
                    }
                    isTracking = value;
                }
            }
        }
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;
            records = new RecordCollection(DataTable, Plot);
            memoryHandler = new Mem();
            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, intervalMinutes, intervalSeconds);
            dispatcherTimer.Start();
            Load();
            Plot.Axes.Add(new DateTimeAxis()
            {
                Position = AxisPosition.Bottom,
            });
            Graph.Model = Plot;
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (IsTracking && gameProcessId != 0)
            {
                if (IsRunning(gameProcessId))
                {
                    GetData();
                    Plot.InvalidatePlot(true);
                    Save();
                }
                else
                {
                    Log("Process with ID: " + gameProcessId + " is no longer running. Did the game crash?");
                    IsTracking = false;
                }
            }
        }

        private void StartStopTracking(object sender, RoutedEventArgs e)
        {
            if (IsTracking && gameProcessId != 0)
            {
                CloseProcess();
            }
            else
            {
                OpenProcess();
            }
        }

        private void CloseProcess()
        {
            memoryHandler.CloseProcess();
            IsTracking = false;
        }

        private void OpenProcess()
        {
            gameProcessId = memoryHandler.GetProcIdFromName(processName);
            if (gameProcessId != 0)
            {
                Log("Process ID: " + gameProcessId);
                //Need to recreate process (required by library code)
                memoryHandler.mProc = new Proc();
                if (memoryHandler.OpenProcess(gameProcessId))
                {
                    IsTracking = true;

                }
                else
                {
                    Log("Could not attach to process " + processName);
                }
            }
            else
            {
                Log("Could not find process " + processName);
            }
        }

        public void Log(string msg)
        {
            Logs.AppendText(String.Format("{0}\t{1}\n", DateTime.Now, msg));
        }

        public void GetData()
        {
            var cash = CreateFloatRecord("Cash", "GSS2-Win64-Shipping.exe+0x040FF6F0,0x30,0x228,0x1A0,0x0,11C");
            var popularity = CreateIntRecord("Popularity", "GSS2-Win64-Shipping.exe+0x04115790,0x130,0x888");
            var moneySpentOnFuel = CreateFloatRecord("Money Spent On Fuel", "GSS2-Win64-Shipping.exe+0x040FF6F0,0x30,0x580,0x1A0,0xE0,0xD8");
            var moneyEarnedOnFuel = CreateFloatRecord("Money Earned On Fuel", "GSS2-Win64-Shipping.exe+0x04115790,0x130,0x790");
            var currentFuelCapacity = CreateFloatRecord("Current Fuel Capacity", "GSS2-Win64-Shipping.exe+0x040FF6F0,0x30,0x228,0x1A0,0x0,0x114");

            var record = new Record()
            {
                Date = DateTime.Now,
                IGT = new InGameTime(),
                SingleRecords = new List<SingleValue>()
                {
                    cash,
                    popularity,
                    moneyEarnedOnFuel,
                    moneySpentOnFuel,
                    currentFuelCapacity
                }
            };
            Records.Add(record);
        }

        private SingleValue CreateIntRecord(string name, string pointerPath)
        {
            int value = memoryHandler.ReadInt(pointerPath);
            var valueRecord = new SingleValue()
            {
                Name = name,
                Value = value,
            };
            return valueRecord;
        }

        private SingleValue CreateFloatRecord(string name, string pointerPath)
        {
            float value = memoryHandler.ReadFloat(pointerPath);
            var valueRecord = new SingleValue()
            {
                Name = name,
                Value = value,
            };
            return valueRecord;
        }

        public void Load()
        {
            if (File.Exists(fileName))
            {
                string content = File.ReadAllText(fileName);
                var records = JsonConvert.DeserializeObject<RecordCollection>(content, settings);
                foreach (var rec in records)
                {
                    Records.Add(rec);
                }
            }
        }

        public void Save()
        {
            using (StreamWriter sw = new StreamWriter(fileName, false))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                string result = JsonConvert.SerializeObject(Records, settings);
                sw.Write(result);
            }
        }

        public static bool IsRunning(int id)
        {
            try 
            {
                Process.GetProcessById(id);
            }
            catch (InvalidOperationException) 
            { 
                return false; 
            }
            catch (ArgumentException) 
            { 
                return false; 
            }
            return true;
        }

        private void RawDataClick(object sender, RoutedEventArgs e)
        {
            DataTable.Visibility = Visibility.Visible;
            LiveGraphs.Visibility = Visibility.Collapsed;
            Settings.Visibility = Visibility.Collapsed;
        }

        private void LiveGraphsClick(object sender, RoutedEventArgs e)
        {
            LiveGraphs.Visibility = Visibility.Visible;
            DataTable.Visibility = Visibility.Collapsed;
            Settings.Visibility = Visibility.Collapsed;
        }

        private void SettingsClick(object sender, RoutedEventArgs e)
        {
            Settings.Visibility = Visibility.Visible;
            LiveGraphs.Visibility = Visibility.Collapsed;
            DataTable.Visibility = Visibility.Collapsed;
        }
    }
}
