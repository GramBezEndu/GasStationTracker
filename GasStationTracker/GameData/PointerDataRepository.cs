using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Data;

namespace GasStationTracker.GameData
{
    public class PointerDataRepository : INotifyPropertyChanged
    {
        private ICollectionView onlineVersionsView;
        private ObservableCollection<string> embeddedVersionCollection = new ObservableCollection<string>();

        public ObservableCollection<PointerData> OnlineRepositoryData { get; private set; } = new ObservableCollection<PointerData>();

        public ObservableCollection<string> OnlineVersionCollection { get; private set; } = new ObservableCollection<string>();

        public ICollectionView OnlineVersionsView
        {
            get => onlineVersionsView;
            set
            {
                if (value != onlineVersionsView)
                {
                    onlineVersionsView = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ObservableCollection<PointerData> EmbeddedData { get; private set; } = new ObservableCollection<PointerData>();

        public ObservableCollection<string> EmbeddedVersionCollection 
        { 
            get => embeddedVersionCollection;
            set
            {
                if (value != embeddedVersionCollection)
                {
                    embeddedVersionCollection = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ICollectionView EmbeddedVersionsView { get; set; }

        public List<PointerData> LocalFileData { get; private set; } = new List<PointerData>();

        public event PropertyChangedEventHandler PropertyChanged;

        public PointerData GetLatestOnlineVersion()
        {
            return OnlineRepositoryData.OrderByDescending(x => x.GameVersion).First();
        }

        public PointerDataRepository()
        {
            SetupEmbeddedData();
            SetupOnlineData();
        }

        private void SetupOnlineData()
        {
            OnlineRepositoryData.CollectionChanged += (o, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (object data in e.NewItems)
                    {
                        PointerData pointerData = (PointerData)data;
                        OnlineVersionCollection.Add(pointerData.GameVersion.ToString());
                    }
                    OnlineVersionsView = CollectionViewSource.GetDefaultView(OnlineVersionCollection);
                    if (OnlineVersionsView != null && OnlineVersionsView.CanSort == true)
                    {
                        OnlineVersionsView.SortDescriptions.Clear();
                        OnlineVersionsView.SortDescriptions.Add(new SortDescription(".", ListSortDirection.Descending));
                    }
                }
            };
        }

        private void SetupEmbeddedData()
        {
            EmbeddedData.CollectionChanged += (o, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (object data in e.NewItems)
                    {
                        PointerData pointerData = (PointerData)data;
                        EmbeddedVersionCollection.Add(pointerData.GameVersion.ToString());
                    }
                    EmbeddedVersionsView = CollectionViewSource.GetDefaultView(EmbeddedVersionCollection);
                    if (EmbeddedVersionsView != null && EmbeddedVersionsView.CanSort == true)
                    {
                        EmbeddedVersionsView.SortDescriptions.Clear();
                        EmbeddedVersionsView.SortDescriptions.Add(new SortDescription(".", ListSortDirection.Descending));
                    }
                }
            };
            EmbeddedData.Add(CreateDataForVersion_1_0_1_37938());
            EmbeddedData.Add(CreateDataForVersion_1_0_1_38259());
        }

        private static PointerData CreateDataForVersion_1_0_1_37938()
        {
            var data = new PointerData(new Version("1.0.1.37938"));
            data.Pointers.Add(GameIdentifiers.CashDisplay, "GSS2-Win64-Shipping.exe+0x040FF6F0,0x30,0x228,0x1A0,0x0,0x11C");
            data.Pointers.Add(GameIdentifiers.PopularityDisplay, "GSS2-Win64-Shipping.exe+0x04115790,0x130,0x888");
            data.Pointers.Add(GameIdentifiers.MoneySpentOnFuelDisplay, "GSS2-Win64-Shipping.exe+0x040FF6F0,0x30,0x580,0x1A0,0xE0,0xD8");
            data.Pointers.Add(GameIdentifiers.MoneyEarnedOnFuelDisplay, "GSS2-Win64-Shipping.exe+0x04115790,0x130,0x790");
            data.Pointers.Add(GameIdentifiers.CurrentFuelDisplay, "GSS2-Win64-Shipping.exe+0x040FF6F0,0x30,0x228,0x1A0,0x0,0x114");
            data.Pointers.Add(GameIdentifiers.IGT, "GSS2-Win64-Shipping.exe+0x04115790,0x130,0x2E4");
            data.Pointers.Add(GameIdentifiers.InGame, "GSS2-Win64-Shipping.exe+0x3FD4A06");
            return data;
        }

        private static PointerData CreateDataForVersion_1_0_1_38259()
        {
            var data = new PointerData(new Version("1.0.1.38259"));
            data.Pointers.Add(GameIdentifiers.CashDisplay, "GSS2-Win64-Shipping.exe+0x041112B0,0x30,0x228,0x1A0,0x0,0x11C");
            data.Pointers.Add(GameIdentifiers.PopularityDisplay, "GSS2-Win64-Shipping.exe+0x04127350,0x130,0x888");
            data.Pointers.Add(GameIdentifiers.MoneySpentOnFuelDisplay, "GSS2-Win64-Shipping.exe+0x041112B0,0x30,0x580,0x1A0,0xE0,0xD8");
            data.Pointers.Add(GameIdentifiers.MoneyEarnedOnFuelDisplay, "GSS2-Win64-Shipping.exe+0x04127350,0x130,0x790");
            data.Pointers.Add(GameIdentifiers.CurrentFuelDisplay, "GSS2-Win64-Shipping.exe+0x041112B0,0x30,0x228,0x1A0,0x0,0x114");
            data.Pointers.Add(GameIdentifiers.IGT, "GSS2-Win64-Shipping.exe+0x04127350,0x130,0x2E4");
            data.Pointers.Add(GameIdentifiers.InGame, "GSS2-Win64-Shipping.exe+0x3FE65C6");
            return data;
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
