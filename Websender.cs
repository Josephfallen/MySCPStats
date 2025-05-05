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
                Log.Info("[SCPStats] No data to upload.");
                return;
            }

            var token = Plugin.Instance.TokenManager.CurrentToken;
            if (string.IsNullOrEmpty(token))
            {
                Log.Warn("[SCPStats] No token available. Skipping upload.");
                return;
            }

            try
            {
                var payload = new
                {
                    server_id = Server.Name,
                    kills = kills,
                    games_played = games
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
                    Log.Info($"[SCPStats] Successfully uploaded {kills.Count} kills and {games.Count} player rounds.");
                }
                else
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    Log.Warn($"[SCPStats] Upload failed: {response.StatusCode}. Response body: {responseBody}");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[SCPStats] Upload error: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
