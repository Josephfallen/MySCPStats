using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;
using Newtonsoft.Json;
using System.Collections.Generic;

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

            var token = Plugin.Instance.TokenManager.CurrentToken;
            if (string.IsNullOrEmpty(token))
            {
                Log.Debug("[SCPStats] No token available. Skipping upload.");
                return;
            }

            try
            {
                // Encrypt player IDs in kills
                foreach (var kill in kills)
                {
                    kill.KillerId = CryptoHelper.Encrypt(kill.KillerId);  // Encrypt KillerId
                    kill.PlayerId = CryptoHelper.Encrypt(kill.PlayerId);  // Encrypt PlayerId
                }

                // Encrypt player IDs in games and convert to object format
                var gamesPlayed = new Dictionary<string, int>();
                foreach (var game in games)
                {
                    string encryptedPlayerId = CryptoHelper.Encrypt(game.Key);  // Encrypt player ID
                    gamesPlayed[encryptedPlayerId] = game.Value;  // Add player ID and game count to dictionary
                }

                var payload = new
                {
                    server_id = Server.Name,
                    kills = kills,
                    games_played = gamesPlayed  // Send gamesPlayed as an object (dictionary)
                };

                string json = JsonConvert.SerializeObject(payload);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using HttpClient client = new();
                client.Timeout = TimeSpan.FromSeconds(5);
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                Log.Debug($"[SCPStats] Request body: {json}");

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

    }
}
