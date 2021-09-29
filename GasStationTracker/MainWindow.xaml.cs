using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
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

namespace GasStationTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public int intervalMinutes = 0;
        public int intervalSeconds = 30;

        string fileName = "Cash.json";

        public ObservableCollection<ValueRecord<float>> Cash { get => cash; private set => cash = value; }
        System.Windows.Threading.DispatcherTimer dispatcherTimer;
        Mem memoryHandler;
        bool isTracking = false;

        #region GssData
        const string processName = "GSS2-Win64-Shipping";
        int gameProcessId;
        private ObservableCollection<ValueRecord<float>> cash = new ObservableCollection<ValueRecord<float>>();

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
            GetData();
            Save();
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

        public bool GetData()
        {
            if (IsTracking && gameProcessId != 0)
            {
                float cash = memoryHandler.ReadFloat("GSS2-Win64-Shipping.exe+0x03BE0920,0x1E0,0x4C0,0x11C");
                var cashRecord = new ValueRecord<float>()
                {
                    Date = DateTime.Now,
                    Name = "Cash",
                    Value = cash,
                };
                //Log("Cash: " + cashRecord);
                Cash.Add(cashRecord);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Load()
        {
            try
            {
                var content = File.ReadAllText(fileName);
                Cash = JsonSerializer.Deserialize<ObservableCollection<ValueRecord<float>>>(content);
            }
            catch
            {

            }
        }

        public void Save()
        {
            using (TextWriter stream = new StreamWriter(fileName, false))
            {
                var content = JsonSerializer.Serialize<ObservableCollection<ValueRecord<float>>>(cash);
                stream.Write(content);
            }
        }
    }
}
