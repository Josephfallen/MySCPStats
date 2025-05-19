using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;
using Newtonsoft.Json.Linq; 

namespace SCPStats
{
    public class TokenManager
    {
        private static readonly HttpClient httpClient = new();
        public string CurrentToken { get; private set; }

        public async Task<bool> FetchTokenAsync()
        {
            try
            {
                Log.Info("[SCPStats] Requesting new token...");

                var json = new JObject
                {
                    ["username"] = "server",
                    ["password"] = "049-is-cool"
                };

                var content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
                httpClient.Timeout = TimeSpan.FromSeconds(5);

                var response = await httpClient.PostAsync("https://myscpstats.com/api/get-token", content);
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Log.Warn($"[SCPStats] Token fetch failed: {response.StatusCode} - {body}");
                    return false;
                }

                var parsed = JObject.Parse(body);
                if (!parsed.TryGetValue("token", out JToken tokenValue))
                {
                    Log.Warn("[SCPStats] Token not found in response.");
                    return false;
                }

                CurrentToken = tokenValue.ToString();
                Log.Info("[SCPStats] Token retrieved successfully.");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"[SCPStats] Token fetch error: {ex.Message}");
                return false;
            }
        }
    }
}
