using System;
using System.Collections.Generic;
using System.Text;

namespace GasStationTracker.GameData
{
    public class PointerData
    {
        public Version GameVersion { get; private set; }

        public Dictionary<string, string> Pointers { get; set; } = new Dictionary<string, string>();

        public PointerData(Version gameVersion)
        {
            GameVersion = gameVersion;
        }
    }
}
