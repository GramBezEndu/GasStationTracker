using GasStationTracker.GameData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml.Linq;

namespace GasStationTracker
{
    public class CheatTableReader
    {
        public string GithubFilePath
        {
            get
            {
                return String.Format("https://raw.githubusercontent.com/{0}/{1}/{2}/{3}", githubUser, repositoryName, branch, filename);
            }
        }

        public List<PointerData> Data { get; set; }

        public event EventHandler OnDataLoaded;

        private const string githubUser = "GramBezEndu";

        private const string repositoryName = "GasStationSimulatorCheatTable";

        private const string branch = "master";

        private const string filename = "GSS2-Win64-Shipping.CT";

        public CheatTableReader()
        {

        }

        public async void GetPointerData()
        {
            using (WebClient client = new WebClient())
            {
                try
                {
                    string text = await client.DownloadStringTaskAsync(GithubFilePath);
                    Data = ParseRawText(text);
                    OnDataLoaded?.Invoke(this, new EventArgs());
                }
                catch
                {
                    MessageBox.Show("Could not get pointer data from online repository", "Online repository error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private List<PointerData> ParseRawText(string rawText)
        {
            List<PointerData> data = new List<PointerData>();
            XDocument document = XDocument.Parse(rawText, LoadOptions.None);
            XElement cheatTable = document.Root;
            IEnumerable<XElement> versionEntries = cheatTable.Elements("CheatEntries").Elements("CheatEntry");

            foreach (XElement versionEntry in versionEntries)
            {
                string currentVersion = versionEntry.Elements("Description").First().Value.Replace("Version: ", String.Empty);
                string cleanedVersionTag = Regex.Replace(currentVersion, "[^0-9. ]", "");
                PointerData versionData = new PointerData(new Version(cleanedVersionTag));

                List<XElement> cheatEntries = new List<XElement>()
                {
                    FindCheatEntryWithName(versionEntry, GameIdentifiers.CashDisplay),
                    FindCheatEntryWithName(versionEntry, GameIdentifiers.MoneySpentOnFuelDisplay),
                    FindCheatEntryWithName(versionEntry, GameIdentifiers.MoneyEarnedOnFuelDisplay),
                    FindCheatEntryWithName(versionEntry, GameIdentifiers.PopularityDisplay),
                    FindCheatEntryWithName(versionEntry, GameIdentifiers.CurrentFuelDisplay),
                    FindCheatEntryWithName(versionEntry, GameIdentifiers.IGT),
                    FindCheatEntryWithName(versionEntry, GameIdentifiers.InGame),
                };
                foreach (XElement entry in cheatEntries)
                {
                    versionData.Pointers.Add(
                        Regex.Replace(entry.Element("Description").Value, "[^A-Za-z0-9. +-x]", ""),
                        GetFullAddress(entry));
                }
                data.Add(versionData);
            }
            return data;
        }

        private XElement FindCheatEntryWithName(XElement versionEntry, string name)
        {
            var cheatEntries = versionEntry.Elements("CheatEntries").Elements("CheatEntry");
            return cheatEntries.First(x => Regex.Replace(x.Element("Description").Value, "[^A-Za-z0-9. +-x]", "") == name);
        }

        private string GetFullAddress(XElement cheatEntry)
        {
            XElement pointerEntry = cheatEntry.Element("CheatEntries").Element("CheatEntry");
            string address = Regex.Replace(pointerEntry.Element("Address").Value, "[^A-Za-z0-9. +-x]", "");
            //Add "0x" string before offset
            address = address.Replace("+", "+0x");
            //If there are any offsets
            if (pointerEntry.Element("Offsets") != null)
            {
                XElement[] offsetsElements = pointerEntry.Element("Offsets").Elements("Offset").ToArray();
                List<string> offsets = new List<string>();
                //Reverse loop due to cheat table structure
                for (int i = offsetsElements.Length - 1; i >= 0; i--)
                {
                    XElement offsetElement = offsetsElements[i];
                    string offset = String.Format(",0x{0}", offsetElement.Value);
                    offsets.Add(offset);
                }
                StringBuilder fullAddressBuilder = new StringBuilder();
                fullAddressBuilder.Append(address);
                foreach (var offset in offsets)
                    fullAddressBuilder.Append(offset);
                return fullAddressBuilder.ToString();
            }
            else
            {
                return address;
            }
        }
    }
}
