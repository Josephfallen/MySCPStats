// -----------------------------------------------------------------------
// <copyright file="Config.cs" company="MySCPStats">
// Copyright (c) joseph_fallen. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Exiled.API.Features;

namespace SCPStats
{
    public static class AutoUpdater
    {
        private const string RepoOwner = "Josephfallen";
        private const string RepoName = "MySCPStats";
        private const string PluginFileName = "SCPStats.dll";

        private static readonly string GithubApiLatestRelease = $"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest";
        private static readonly string PluginPath = Path.Combine(Paths.Plugins, PluginFileName);

        public static async Task RunAsync()
        {
            try
            {
                Log.Info("[AutoUpdater] Checking GitHub for plugin updates...");

                using HttpClient client = new();
                client.DefaultRequestHeaders.Add("User-Agent", "MySCPStats-AutoUpdater");

                var response = await client.GetAsync(GithubApiLatestRelease);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var release = JsonDocument.Parse(json).RootElement;

                string latestVersion = release.GetProperty("tag_name").GetString()?.TrimStart('v');
                string currentVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString();

                if (string.IsNullOrWhiteSpace(latestVersion))
                {
                    Log.Warn("[AutoUpdater] Could not determine latest version tag.");
                    return;
                }

                if (string.Equals(latestVersion, currentVersion, StringComparison.OrdinalIgnoreCase))
                {
                    Log.Info("[AutoUpdater] Plugin is up to date.");
                    return;
                }

                Log.Warn($"[AutoUpdater] Update available: {latestVersion} (current: {currentVersion})");

                foreach (var asset in release.GetProperty("assets").EnumerateArray())
                {
                    string name = asset.GetProperty("name").GetString();
                    string downloadUrl = asset.GetProperty("browser_download_url").GetString();

                    if (name.Equals(PluginFileName, StringComparison.OrdinalIgnoreCase))
                    {
                        string tempPath = PluginPath + ".update";

                        Log.Info("[AutoUpdater] Downloading updated plugin...");

                        using var downloadStream = await client.GetStreamAsync(downloadUrl);
                        using var fileStream = File.Create(tempPath);
                        await downloadStream.CopyToAsync(fileStream);

                        // Replace old file
                        File.Delete(PluginPath);
                        File.Move(tempPath, PluginPath);

                        Log.Info("[AutoUpdater] Update downloaded successfully! Restart the server to apply.");
                        return;
                    }
                }

                Log.Warn("[AutoUpdater] Plugin .dll not found in the release assets.");
            }
            catch (Exception ex)
            {
                Log.Error($"[AutoUpdater] Update failed: {ex.Message}");
            }
        }
    }
}
