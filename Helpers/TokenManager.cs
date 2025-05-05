using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;

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

                var sb = new StringBuilder();
                sb.Append("{")
                  .Append("\"username\":\"server\",")
                  .Append("\"password\":\"049-is-cool\"")
                  .Append("}");

                var content = new StringContent(sb.ToString(), Encoding.UTF8, "application/json");
                httpClient.Timeout = TimeSpan.FromSeconds(5);

                var response = await httpClient.PostAsync("https://myscpstats.com/api/get-token", content);

                if (!response.IsSuccessStatusCode)
                {
                    Log.Warn($"[SCPStats] Token fetch failed: {response.StatusCode}");
                    return false;
                }

                var body = await response.Content.ReadAsStringAsync();
                var token = ExtractToken(body);
                if (string.IsNullOrEmpty(token))
                {
                    Log.Warn("[SCPStats] Token not found in response.");
                    return false;
                }

                CurrentToken = token;
                Log.Info("[SCPStats] Token retrieved successfully.");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"[SCPStats] Token fetch error: {ex.Message}");
                return false;
            }
        }

        private string ExtractToken(string json)
        {
            const string tokenKey = "\"token\":\"";
            int start = json.IndexOf(tokenKey);
            if (start == -1)
                return null;

            start += tokenKey.Length;
            int end = json.IndexOf('"', start);
            if (end == -1)
                return null;

            return json.Substring(start, end - start);
        }
    }
}
