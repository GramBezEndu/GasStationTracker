using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using Memory;
using Newtonsoft.Json;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace GasStationTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public int intervalMinutes = 0;
        public int intervalSeconds = 5;

        string fileName = "Data.json";

        JsonSerializerSettings settings = new JsonSerializerSettings 
        {
            NullValueHandling = NullValueHandling.Ignore,
            DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
            Formatting = Formatting.None,
            TypeNameHandling = TypeNameHandling.Auto,
        };

        public SessionStatistics SessionStats { get; private set; } = new SessionStatistics();
        public PlotModel Plot { get; private set; } = new PlotModel();
        public RecordCollection Records { get => records; private set => records = value; }
        private RecordCollection records;
        System.Windows.Threading.DispatcherTimer dispatcherTimer;
        Mem memoryHandler;

        #region GssData
        const string processName = "GSS2-Win64-Shipping";
        public static string CashDisplay = "Cash";
        public static string PopularityDisplay = "Popularity";
        const string moneySpentFuelDisplay = "Money Spent On Fuel";
        const string moneyEarnedFuelDisplay = "Money Earned On Fuel";
        const string currentFuelDisplay = "Current Fuel Capacity";
        int gameProcessId;
        bool isTracking = false;

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
                        SessionStats.StartTime = DateTime.Now;
                    }
                    else
                    {
                        StartStop.Content = "Start tracking";
                        Log("Process was closed " + processName);
                        SessionStats.EndSession();
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
            Records = new RecordCollection(DataTable, Plot);
            SessionStats.Records = Records;
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
            Plot.Axes.Add(new LinearAxis()
            {
                Position = AxisPosition.Left,
            });
            Graph.Model = Plot;
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (IsTracking && gameProcessId != 0)
            {
                if (memoryHandler.GetProcIdFromName(processName) != 0)
                {
                    if (InGame())
                    {
                        GetData();
                        Plot.InvalidatePlot(true);
                        SessionStats?.Update();
                        Save();
                    }
                    else
                    {
                        Log("In Game Flag is set to false. Load your save file to start tracking.");
                    }
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
                    if (!InGame())
                        Log("In Game Flag is set to false. Load your save file to start tracking.");
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
            var cash = CreateFloatRecord(MainWindow.CashDisplay, "GSS2-Win64-Shipping.exe+0x040FF6F0,0x30,0x228,0x1A0,0x0,11C");
            var popularity = CreateIntRecord(MainWindow.PopularityDisplay, "GSS2-Win64-Shipping.exe+0x04115790,0x130,0x888");
            var moneySpentOnFuel = CreateFloatRecord(MainWindow.moneySpentFuelDisplay, "GSS2-Win64-Shipping.exe+0x040FF6F0,0x30,0x580,0x1A0,0xE0,0xD8");
            var moneyEarnedOnFuel = CreateFloatRecord(MainWindow.moneyEarnedFuelDisplay, "GSS2-Win64-Shipping.exe+0x04115790,0x130,0x790");
            var currentFuelCapacity = CreateFloatRecord(MainWindow.currentFuelDisplay, "GSS2-Win64-Shipping.exe+0x040FF6F0,0x30,0x228,0x1A0,0x0,0x114");
            var igt = memoryHandler.ReadFloat("GSS2-Win64-Shipping.exe+0x04115790,0x130,0x2E4");

            var record = new Record()
            {
                Date = DateTime.Now,
                IGT = new InGameTime(igt),
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
                if (records != null)
                {
                    foreach (var rec in records)
                    {
                        Records.Add(rec);
                    }
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

        public bool InGame()
        {
            var value = memoryHandler.ReadByte("GSS2-Win64-Shipping.exe+0x3FD4A06");
            if (value == 0)
                return false;
            else
                return true;
        }

        private void UpdateGraphLineSeries(string currentFilter)
        {
            ElementCollection<Series> series = Plot.Series;
            foreach (var serie in series)
            {
                serie.IsVisible = false;
            }
            var currentSerie = series.FirstOrDefault(x => x.Title == currentFilter);
            if (currentSerie != null)
            {
                if (currentSerie.IsVisible == false)
                {
                    currentSerie.IsVisible = true;
                    AutoScaleGraph(this.Plot, this.Records);
                    Plot.InvalidatePlot(true);
                }
            }
        }

        public static void AutoScaleGraph(PlotModel model, RecordCollection records)
        {
            IEnumerable<Record> orderedByDate = records.OrderBy(x => x.Date);
            //IEnumerable<Record> orderedByValue = records.OrderBy(x => x.Date);
            if (orderedByDate.Count() >= 1 && model.Axes.Count() >= 1)
            {
                model.Axes[0].Reset();
                model.Axes[0].Minimum = DateTimeAxis.ToDouble(orderedByDate.ElementAt(0).Date);
                model.Axes[0].Maximum = DateTimeAxis.ToDouble(orderedByDate.Last().Date + new TimeSpan(0, 0, 60));
                model.Axes[1].Reset();
                //model.Axes[1].Minimum = LinearAxis.ToDouble()
            }
        }

        #region Buttons
        private void SessionStatsClick(object sender, RoutedEventArgs e)
        {
            SessionStatistics.Visibility = Visibility.Visible;
            DataTable.Visibility = Visibility.Collapsed;
            LiveGraphs.Visibility = Visibility.Collapsed;
            Settings.Visibility = Visibility.Collapsed;
        }

        private void RawDataClick(object sender, RoutedEventArgs e)
        {
            DataTable.Visibility = Visibility.Visible;
            LiveGraphs.Visibility = Visibility.Collapsed;
            Settings.Visibility = Visibility.Collapsed;
            SessionStatistics.Visibility = Visibility.Collapsed;
        }

        private void LiveGraphsClick(object sender, RoutedEventArgs e)
        {
            LiveGraphs.Visibility = Visibility.Visible;
            DataTable.Visibility = Visibility.Collapsed;
            Settings.Visibility = Visibility.Collapsed;
            SessionStatistics.Visibility = Visibility.Collapsed;
        }

        private void SettingsClick(object sender, RoutedEventArgs e)
        {
            Settings.Visibility = Visibility.Visible;
            LiveGraphs.Visibility = Visibility.Collapsed;
            DataTable.Visibility = Visibility.Collapsed;
            SessionStatistics.Visibility = Visibility.Collapsed;
        }

        private void CashGraphClick(object sender, RoutedEventArgs e)
        {
            UpdateGraphLineSeries(MainWindow.CashDisplay);
            AutoScaleGraph(Plot, Records);
            Plot.InvalidatePlot(true);
        }

        private void PopularityGraphClick(object sender, RoutedEventArgs e)
        {
            UpdateGraphLineSeries(MainWindow.PopularityDisplay);
            AutoScaleGraph(Plot, Records);
            Plot.InvalidatePlot(true);
        }

        private void MoneyEarnedOnFuelClick(object sender, RoutedEventArgs e)
        {
            UpdateGraphLineSeries(MainWindow.moneyEarnedFuelDisplay);
            AutoScaleGraph(Plot, Records);
            Plot.InvalidatePlot(true);
        }

        private void MoneySpentOnFuelClick(object sender, RoutedEventArgs e)
        {
            UpdateGraphLineSeries(MainWindow.moneySpentFuelDisplay);
            AutoScaleGraph(Plot, Records);
            Plot.InvalidatePlot(true);
        }

        private void CurrentFuelClick(object sender, RoutedEventArgs e)
        {
            UpdateGraphLineSeries(MainWindow.currentFuelDisplay);
            AutoScaleGraph(Plot, Records);
            Plot.InvalidatePlot(true);
        }

        #endregion
    }
}
