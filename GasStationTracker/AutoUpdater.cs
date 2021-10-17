using Octokit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace GasStationTracker
{
    public class AutoUpdater
    {
        public async System.Threading.Tasks.Task CheckGitHubNewerVersion(Version currentVersion)
        {
            //Get all releases from GitHub
            //Source: https://octokitnet.readthedocs.io/en/latest/getting-started/
            GitHubClient client = new GitHubClient(new ProductHeaderValue("GasStationTrackerDesktopApp"));
            IReadOnlyList<Release> releases = await client.Repository.Release.GetAll("GramBezEndu", "GasStationTracker");

            //Setup the versions (remove first character "v")
            Version latestGitHubVersion = new Version(releases[0].TagName.Remove(0, 1));

            //Compare the Versions
            //Source: https://stackoverflow.com/questions/7568147/compare-version-numbers-without-using-split-function
            int versionComparison = currentVersion.CompareTo(latestGitHubVersion);
            if (versionComparison < 0)
            {
                StringBuilder content = new StringBuilder();
                content.AppendLine(String.Format("Current version: {0}", currentVersion.ToString()));
                content.AppendLine(String.Format("Newest version: {0}", latestGitHubVersion.ToString()));
                content.AppendLine();
                content.AppendLine("Auto update is not implemented in current version");
                content.AppendLine();
                content.AppendLine("Visit github to download newest version");
                //The version on GitHub is more up to date than this local release.
                MessageBox.Show(content.ToString(), "New version!", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else if (versionComparison > 0)
            {
                //This local version is greater than the release version on GitHub.
                //MessageBox.Show("Private version PogChamp", "Private version PogChamp", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                //This local Version and the Version on GitHub are equal.
                //MessageBox.Show("Up to date version", "Up to date version", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
