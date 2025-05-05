using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;

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

            try
            {
                var sb = new StringBuilder();

                sb.Append("{\"server_id\":\"").Append(Server.Name).Append("\",");

                sb.Append("\"kills\":[");
                for (int i = 0; i < kills.Count; i++)
                {
                    var k = kills[i];
                    sb.Append("{")
                      .Append("\"PlayerName\":\"").Append(Escape(k.PlayerName)).Append("\",")
                      .Append("\"PlayerId\":\"").Append(Escape(k.PlayerId)).Append("\",")
                      .Append("\"PlayerClass\":\"").Append(Escape(k.PlayerClass)).Append("\",")
                      .Append("\"KillerName\":\"").Append(Escape(k.KillerName)).Append("\",")
                      .Append("\"KillerId\":\"").Append(Escape(k.KillerId)).Append("\",")
                      .Append("\"KillerClass\":\"").Append(Escape(k.KillerClass)).Append("\",")
                      .Append("\"DamageType\":\"").Append(Escape(k.DamageType)).Append("\",")
                      .Append("\"WeaponOrCause\":\"").Append(Escape(k.WeaponOrCause)).Append("\",")
                      .Append("\"Time\":\"").Append(k.Time).Append("\",")
                      .Append("\"RoundTimeSeconds\":").Append(k.RoundTimeSeconds.ToString("F2"))
                      .Append("}");
                    if (i < kills.Count - 1) sb.Append(",");
                }
                sb.Append("],");

                sb.Append("\"games_played\":{");
                int index = 0;
                foreach (var kvp in games)
                {
                    sb.Append("\"").Append(Escape(kvp.Key)).Append("\":").Append(kvp.Value);
                    if (++index < games.Count) sb.Append(",");
                }
                sb.Append("}}");

                var content = new StringContent(sb.ToString(), Encoding.UTF8, "application/json");

                // ✅ FIXED LINE
                var token = Plugin.Instance.TokenManager.CurrentToken;
                if (string.IsNullOrEmpty(token))
                {
                    Log.Warn("[SCPStats] No token available. Skipping upload.");
                    return;
                }

                using HttpClient client = new();
                client.Timeout = TimeSpan.FromSeconds(5);
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                // Log the request body for debugging
                Log.Debug($"[SCPStats] Request body: {sb.ToString()}");

                var response = await client.PostAsync("https://myscpstats.com/api/uploadKills", content);

                // Log response status for debugging
                Log.Debug($"[SCPStats] Response status code: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    Log.Info($"[SCPStats] Successfully uploaded {kills.Count} kills and {games.Count} player rounds.");
                }
                else
                {
                    // Log the response content (body) for debugging in case of failure
                    var responseBody = await response.Content.ReadAsStringAsync();
                    Log.Warn($"[SCPStats] Upload failed: {response.StatusCode}. Response body: {responseBody}");
                }
            }
            catch (Exception ex)
            {
                // Log exception details for debugging
                Log.Error($"[SCPStats] Upload error: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public static string Escape(string value) => value.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}
