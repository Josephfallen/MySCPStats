// -----------------------------------------------------------------------
// <copyright file="AutoUpdater.cs" company="MySCPStats">
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
        private const string PluginFileName = "myscpstats.dll";

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
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                string currentVersion = $"{version.Major}.{version.Minor}.{version.Build}";


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
                        // Use a different file name for temporary download that doesn't add to the extension
                        string tempDir = Path.GetDirectoryName(PluginPath);
                        string tempFileName = "temp_" + PluginFileName;
                        string tempPath = Path.Combine(tempDir, tempFileName);

                        Log.Info("[AutoUpdater] Downloading updated plugin...");

                        using var downloadStream = await client.GetStreamAsync(downloadUrl);
                        using var fileStream = File.Create(tempPath);
                        await downloadStream.CopyToAsync(fileStream);

                        // Make sure fileStream is closed before replacing files
                        fileStream.Close();

                        try
                        {
                            // Replace old file with appropriate error handling
                            if (File.Exists(PluginPath))
                            {
                                File.Delete(PluginPath);
                            }
                            
                            File.Move(tempPath, PluginPath);
                            Log.Info("[AutoUpdater] Update downloaded successfully! Restart the server to apply.");
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"[AutoUpdater] Failed to replace plugin file: {ex.Message}");
                            
                            // Try to clean up the temp file if possible
                            if (File.Exists(tempPath))
                            {
                                try
                                {
                                    File.Delete(tempPath);
                                }
                                catch
                                {
                                    // Ignore cleanup errors
                                }
                            }
                        }
                        
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
