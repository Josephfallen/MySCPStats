// -----------------------------------------------------------------------
// <copyright file="Websender.cs" company="MySCPStats">
// Copyright (c) joseph_fallen. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http.Headers;

namespace SCPStats
{
    public static class WebSender
    {
        public static async Task UploadKillsWithTokenAsync()
        {
            var kills = KillLogger.RetrieveAndClear();
            var games = PlayerStatsTracker.GetAll();

            if (kills.Count == 0 && games.Count == 0)
            {
                Log.Debug("[SCPStats] No data to upload.");
                return;
            }

            var token = Plugin.Instance.VerificationManager.CurrentToken;
            if (string.IsNullOrEmpty(token))
            {
                Log.Warn("[SCPStats] No token available. Skipping upload.");
                return;
            }

            try
            {
                // Encrypt PlayerId and KillerId in kills
                foreach (var kill in kills)
                {
                    kill.PlayerId = EncryptPlayerId(kill.PlayerId);
                    kill.KillerId = EncryptPlayerId(kill.KillerId);
                    kill.WeaponOrCause = EncryptPlayerId(kill.WeaponOrCause); // Encrypt Steam ID in WeaponOrCause as well
                }

                // Encrypt the PlayerId for the games_played data
                var encryptedGamesPlayed = new Dictionary<string, int>();
                foreach (var game in games)
                {
                    encryptedGamesPlayed[EncryptPlayerId(game.Key)] = game.Value;
                }

                var payload = new
                {
                    server_id = Server.Name,
                    kills = kills,
                    games_played = encryptedGamesPlayed
                };

                string json = JsonConvert.SerializeObject(payload);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using HttpClient client = new();
                client.Timeout = TimeSpan.FromSeconds(5);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.PostAsync("https://myscpstats.com/api/uploadKills", content);

                Log.Debug($"[SCPStats] Response status code: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    Log.Debug($"[SCPStats] Successfully uploaded {kills.Count} kills and {games.Count} player rounds.");
                }
                else
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    Log.Debug($"[SCPStats] Upload failed: {response.StatusCode}. Response body: {responseBody}");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[SCPStats] Upload error: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private static string EncryptPlayerId(string input)
        {
            // This method encrypts the PlayerId and Steam IDs, replacing them with the encrypted versions
            if (string.IsNullOrEmpty(input)) return input;

            // Encrypt Steam IDs like '76561198880710561@steam'
            string pattern = @"\d{17}@steam"; // Pattern to match Steam IDs
            var regex = new System.Text.RegularExpressions.Regex(pattern);

            // Replace all occurrences of Steam ID with encrypted version
            return regex.Replace(input, match => EncryptSteamId(match.Value));
        }

        private static string EncryptSteamId(string steamId)
        {
            // Here you can implement your encryption logic. Example:
            var encryptedSteamId = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(steamId));
            return encryptedSteamId;
        }
    }
}
