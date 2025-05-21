// -----------------------------------------------------------------------
// <copyright file="VerificationManager.cs" company="MySCPStats">
// Copyright (c) joseph_fallen. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;
using Newtonsoft.Json.Linq;

namespace SCPStats
{
    public class VerificationManager
    {
        private static readonly HttpClient httpClient = new();

        public string CurrentToken { get; private set; }

        public async Task<bool> AutoVerify()
        {
            try
            {
                Log.Info("[SCPStats] Fetching public IP for verification...");

                httpClient.Timeout = TimeSpan.FromSeconds(5);
                var ipResponse = await httpClient.GetAsync("https://api.ipify.org");
                if (!ipResponse.IsSuccessStatusCode)
                {
                    Log.Warn($"[SCPStats] Failed to get public IP: {ipResponse.StatusCode}");
                    return false;
                }

                string publicIp = (await ipResponse.Content.ReadAsStringAsync()).Trim();
                if (string.IsNullOrEmpty(publicIp))
                {
                    Log.Warn("[SCPStats] Public IP is empty.");
                    return false;
                }

                Log.Info($"[SCPStats] Public IP obtained: {publicIp}");

                var json = new JObject
                {
                    ["ip_address"] = publicIp
                };

                var content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");

                Log.Info("[SCPStats] Sending verification request to server...");
                var verifyResponse = await httpClient.PostAsync("https://myscpstats.com/api/admin/verify", content);

                var body = await verifyResponse.Content.ReadAsStringAsync();

                if (!verifyResponse.IsSuccessStatusCode)
                {
                    Log.Warn($"[SCPStats] Verification failed: {verifyResponse.StatusCode} - {body}");
                    return false;
                }

                var parsed = JObject.Parse(body);
                if (!parsed.TryGetValue("token", out JToken tokenValue))
                {
                    Log.Warn("[SCPStats] Token not found in verification response.");
                    return false;
                }

                CurrentToken = tokenValue.ToString();
                Log.Info("[SCPStats] Verification successful, token stored.");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"[SCPStats] Verification error: {ex}");
                return false;
            }
        }
    }
}
