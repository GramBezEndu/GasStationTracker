using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using GasStationTracker.Controls;
using GasStationTracker.Converters;
using GasStationTracker.GameData;
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
        public SessionStatistics SessionStats { get; private set; } = new SessionStatistics();

        public PlotModel Plot { get; private set; } = new PlotModel();

        public PointerDataRepository PointersRepository { get; set; } = new PointerDataRepository();

        public RecordCollection Records { get => records; private set => records = value; }

        public Version CurrentVersion
        {
            get
            {
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                return new Version(String.Format("{0}.{1}.{2}", version.Major, version.Minor, version.Build));
            }
        }

        public string VersionDisplay
        {
            get
            {
                return CurrentVersion.ToString();
            }
        }

        private RecordCollection records;

        private readonly int intervalMinutes = 0;

        private readonly int intervalSeconds = 5;

        private readonly string fileName = "Data.json";

        private CheatTableReader cheatTableReader = new CheatTableReader();

        private readonly JsonSerializerSettings settings = new JsonSerializerSettings 
        {
            NullValueHandling = NullValueHandling.Ignore,
            DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
            Formatting = Formatting.None,
            TypeNameHandling = TypeNameHandling.Auto,
        };

        private System.Windows.Threading.DispatcherTimer dispatcherTimer;

        private readonly Mem memoryHandler;

        #region GameHandling
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
                        Log("Attatched to process " + GameIdentifiers.ProcessName);
                        SessionStats.StartTime = DateTime.Now;
                    }
                    else
                    {
                        StartStop.Content = "Start tracking";
                        Log("Process was closed " + GameIdentifiers.ProcessName);
                        SessionStats.EndSession();
                    }
                    isTracking = value;
                }
            }
        }

        private int gameProcessId;

        private bool isTracking = false;
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            Records = new RecordCollection(RawData.DataTable, Plot);
            SessionStats.Records = Records;
            memoryHandler = new Mem();
            Load();
            AddPlotAxes();
            Plot.TextColor = OxyColors.Black;
            Plot.LegendTextColor = OxyColors.Black;
            Plot.LegendFontSize = 14;
            var legendBackground = System.Drawing.ColorTranslator.FromHtml("#B9A8CE");
            Plot.LegendBackground = OxyColor.FromArgb(legendBackground.A, legendBackground.R, legendBackground.G, legendBackground.B);
            Plot.LegendPlacement = LegendPlacement.Inside;
            LiveGraphs.Graph.Model = Plot;
            cheatTableReader.OnDataLoaded += (o, e) => SetData();
            cheatTableReader.GetPointerData();
            if (UserSettings.Default.AutoUpdate)
                CheckVersion();
            InitTimer();
        }

        private void SetData()
        {
            foreach (var data in cheatTableReader.Data)
                PointersRepository.OnlineRepositoryData.Add(data);
            ListView onlineVersionList = Settings.PointerSettingsView.OnlineVersionList;
            string onlineVersionSelected = onlineVersionList.Items.Cast<string>().First(x => x == UserSettings.Default.OnlineRepositoryVersion);
            onlineVersionList.SelectedItem = onlineVersionSelected;
            ListView embeddedVersionList = Settings.PointerSettingsView.EmbeddedVersionList;
            string embeddedVersionSelected = embeddedVersionList.Items.Cast<string>().First(x => x == UserSettings.Default.EmbeddedVersion);
            embeddedVersionList.SelectedItem = embeddedVersionSelected;
        }

        private async void CheckVersion()
        {
            await new AutoUpdater().CheckGitHubNewerVersion(CurrentVersion);
        }

        private void AddPlotAxes()
        {
            Plot.Axes.Add(new DateTimeAxis()
            {
                Position = AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.Automatic,
            });
            Plot.Axes.Add(new LinearAxis()
            {
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Automatic,
            });
        }

        private void InitTimer()
        {
            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, intervalMinutes, intervalSeconds);
            dispatcherTimer.Start();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (IsTracking && gameProcessId != 0)
            {
                if (memoryHandler.GetProcIdFromName(GameIdentifiers.ProcessName) != 0)
                {
                    PointerData pointerList = GetPointerList();
                    if (pointerList != null)
                    {
                        if (IsInGame(pointerList))
                        {
                            GetData(pointerList);
                            Plot.InvalidatePlot(true);
                            SessionStats?.Update();
                            Save();
                        }
                        else
                        {
                            Log("In Game Flag is set to false. Load your save file to start tracking.");
                            return;
                        }
                    }
                    else
                    {
                        Log("Could not find pointer data with selected pointer source and game version");
                        return;
                    }
                }
                else
                {
                    Log("Process with ID: " + gameProcessId + " is no longer running. Did the game crash?");
                    IsTracking = false;
                    return;
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
            gameProcessId = memoryHandler.GetProcIdFromName(GameIdentifiers.ProcessName);
            PointerData pointerList = GetPointerList();
            if (pointerList == null)
            {
                Log("Could not find pointer data with selected pointer source and game version");
                return;
            }
            if (gameProcessId != 0)
            {
                Log("Process ID: " + gameProcessId);
                //Need to recreate process (required by library code)
                memoryHandler.mProc = new Proc();
                if (memoryHandler.OpenProcess(gameProcessId))
                {
                    IsTracking = true;
                    if (!IsInGame(pointerList))
                    {
                        Log("In Game Flag is set to false. Load your save file to start tracking.");
                        return;
                    }
                }
                else
                {
                    Log("Could not attach to process " + GameIdentifiers.ProcessName);
                    return;
                }
            }
            else
            {
                Log("Could not find process " + GameIdentifiers.ProcessName);
                return;
            }
        }

        public void Log(string msg)
        {
            Logs.AppendText(String.Format("{0}\t{1}\n", DateTime.Now, msg));
        }

        public void GetData(PointerData pointerList)
        {
            if (pointerList != null)
            {
                var cash = CreateFloatRecord(GameIdentifiers.CashDisplay, pointerList.Pointers[GameIdentifiers.CashDisplay]);
                var popularity = CreateIntRecord(GameIdentifiers.PopularityDisplay, pointerList.Pointers[GameIdentifiers.PopularityDisplay]);
                var moneySpentOnFuel = CreateFloatRecord(GameIdentifiers.MoneySpentOnFuelDisplay, pointerList.Pointers[GameIdentifiers.MoneySpentOnFuelDisplay]);
                var moneyEarnedOnFuel = CreateFloatRecord(GameIdentifiers.MoneyEarnedOnFuelDisplay, pointerList.Pointers[GameIdentifiers.MoneyEarnedOnFuelDisplay]);
                var currentFuelCapacity = CreateFloatRecord(GameIdentifiers.CurrentFuelDisplay, pointerList.Pointers[GameIdentifiers.CurrentFuelDisplay]);
                var igt = memoryHandler.ReadFloat(pointerList.Pointers[GameIdentifiers.IGT]);

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
        }

        private PointerData GetPointerList()
        {
            ObservableCollection<PointerData> data = null;
            PointerData pointerList = null;
            PointerSource pointerSrc = (PointerSource)PointerSourceConverter.Convert(UserSettings.Default.PointerSource);
            switch (pointerSrc)
            {
                case PointerSource.OnlineRepository:
                    data = PointersRepository.OnlineRepositoryData;
                    pointerList = data.FirstOrDefault(x => x.GameVersion.ToString() == UserSettings.Default.OnlineRepositoryVersion);
                    break;
                case PointerSource.EmbeddedInApplication:
                    data = PointersRepository.EmbeddedData;
                    pointerList = data.FirstOrDefault(x => x.GameVersion.ToString() == UserSettings.Default.EmbeddedVersion);
                    break;
            }
            return pointerList;
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

        public bool IsInGame(PointerData pointerList)
        {
            int value = memoryHandler.ReadByte(pointerList.Pointers[GameIdentifiers.InGame]);
            if (value == 0)
                return false;
            else
                return true;
        }

        #region Buttons
        private void SessionStatsClick(object sender, RoutedEventArgs e)
        {
            SessionStatistics.Visibility = Visibility.Visible;
            RawData.Visibility = Visibility.Collapsed;
            LiveGraphs.Visibility = Visibility.Collapsed;
            Settings.Visibility = Visibility.Collapsed;
        }

        private void RawDataClick(object sender, RoutedEventArgs e)
        {
            RawData.Visibility = Visibility.Visible;
            LiveGraphs.Visibility = Visibility.Collapsed;
            Settings.Visibility = Visibility.Collapsed;
            SessionStatistics.Visibility = Visibility.Collapsed;
        }

        private void LiveGraphsClick(object sender, RoutedEventArgs e)
        {
            LiveGraphs.Visibility = Visibility.Visible;
            RawData.Visibility = Visibility.Collapsed;
            Settings.Visibility = Visibility.Collapsed;
            SessionStatistics.Visibility = Visibility.Collapsed;
        }

        private void SettingsClick(object sender, RoutedEventArgs e)
        {
            Settings.Visibility = Visibility.Visible;
            LiveGraphs.Visibility = Visibility.Collapsed;
            RawData.Visibility = Visibility.Collapsed;
            SessionStatistics.Visibility = Visibility.Collapsed;
        }
        #endregion

        private void DragWindow(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void CloseApplication(object sender, RoutedEventArgs e)
        {
            UserSettings.Default.PointerSource = GetPointerSourceFromView();
            string checkedOnlineVersion = Settings.PointerSettingsView.OnlineVersionList.SelectedItem.ToString();
            UserSettings.Default.OnlineRepositoryVersion = checkedOnlineVersion;
            string checkedEmbeddedVersion = Settings.PointerSettingsView.EmbeddedVersionList.SelectedItem.ToString();
            UserSettings.Default.EmbeddedVersion = checkedEmbeddedVersion;
            UserSettings.Default.Save();
            Application.Current.Shutdown();
        }

        private string GetPointerSourceFromView()
        {
            if (Settings.PointerSettingsView.EmbeddedButton.IsChecked == true)
            {
                return PointerSourceConverter.Convert(PointerSource.EmbeddedInApplication).ToString();
            }
            else
            {
                return PointerSourceConverter.Convert(PointerSource.OnlineRepository).ToString();
            }
        }

        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximuzeWindow(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
                WindowState = WindowState.Maximized;
            else
                WindowState = WindowState.Normal;
        }
    }
}
