using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace GasStationTracker.GameData
{
    public class PointerDataRepository
    {
        public ObservableCollection<PointerData> OnlineRepositoryData { get; set; } = new ObservableCollection<PointerData>();

        public ObservableCollection<PointerData> EmbeddedData { get; private set; } = new ObservableCollection<PointerData>();

        public ObservableCollection<string> EmbeddedVersionCollection { get; set; } = new ObservableCollection<string>();

        public List<PointerData> LocalFileData { get; private set; } = new List<PointerData>();

        public PointerData GetLatestOnlineVersion()
        {
            return OnlineRepositoryData.OrderByDescending(x => x.GameVersion).First();
        }

        public PointerDataRepository()
        {
            EmbeddedData.Add(CreateDataForVersion_1_0_1_37938());
            EmbeddedData.Add(CreateDataForVersion_1_0_1_38259());
            foreach (PointerData data in EmbeddedData.OrderByDescending(x => x.GameVersion))
            {
                EmbeddedVersionCollection.Add(data.GameVersion.ToString());
            }
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
    }
}
