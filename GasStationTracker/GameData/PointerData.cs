namespace GasStationTracker.GameData
{
    using System;
    using System.Collections.Generic;

    public class PointerData
    {
        public PointerData(Version gameVersion)
        {
            GameVersion = gameVersion;
        }

        public Version GameVersion { get; private set; }

        public Dictionary<string, string> Pointers { get; set; } = new Dictionary<string, string>();
    }
}
