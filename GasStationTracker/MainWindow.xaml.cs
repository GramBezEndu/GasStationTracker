using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

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

        ITraceWriter traceWriter = new MemoryTraceWriter();
        JsonSerializer serializer = new JsonSerializer();
        JsonSerializerSettings settings = new JsonSerializerSettings 
        {
            NullValueHandling = NullValueHandling.Ignore,
            DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.Auto,
        };

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

            serializer.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
            serializer.NullValueHandling = NullValueHandling.Ignore;
            serializer.Formatting = Formatting.Indented;

            settings.TraceWriter = traceWriter;
            //settings.Converters.Add(new SingleValueConverter());

            DataContext = this;
            records = new RecordCollection(DataTable);
            memoryHandler = new Mem();
            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, intervalMinutes, intervalSeconds);
            dispatcherTimer.Start();
            Load();
            Log("LOGS:");
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (IsTracking && gameProcessId != 0)
            {
                GetData();
                Save();
            }
        }

        private void StartStopTracking(object sender, RoutedEventArgs e)
        {
            if (IsTracking && gameProcessId != 0)
            {
                memoryHandler.CloseProcess();
                IsTracking = false;
            }
            else
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
        }

        public void Log(string msg)
        {
            Logs.AppendText(String.Format("{0}\t{1}\n", DateTime.Now, msg));
        }

        public void GetData()
        {
            var cash = CreateFloatRecord("Cash", "GSS2-Win64-Shipping.exe+0x040FF6F0,0x30,0x228,0x1A0,0x0,11C");
            var popularity = CreateIntRecord("Popularity", "GSS2-Win64-Shipping.exe+0x04115790,0x130,0x888");
            var moneySpentOnFuel = CreateFloatRecord("Money Earned On Fuel", "GSS2-Win64-Shipping.exe+0x0408A2B8,0x0,0x110,0x6B0,0xD8");
            var moneyEarnedOnFuel = CreateFloatRecord("Money Spent On Fuel", "GSS2-Win64-Shipping.exe+0x04115790,0x130,0x790");
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

        private SingleValue<int> CreateIntRecord(string name, string pointerPath)
        {
            int value = memoryHandler.ReadInt(pointerPath);
            var valueRecord = new SingleValue<int>()
            {
                Name = name,
                Value = value,
            };
            return valueRecord;
        }

        private SingleValue<float> CreateFloatRecord(string name, string pointerPath)
        {
            float value = memoryHandler.ReadFloat(pointerPath);
            var valueRecord = new SingleValue<float>()
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
    }
}
