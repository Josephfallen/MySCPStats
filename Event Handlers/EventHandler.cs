// -----------------------------------------------------------------------
// <copyright file="EventHandler.cs" company="MySCPStats">
// Copyright (c) joseph_fallen. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------
using System.Threading.Tasks;
using Exiled.API.Features;
using MEC;

namespace SCPStats
{
    public static class EventHandler
    {
        public static void Register()
        {
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;  // Fully qualified reference for Server.RoundStarted
            Exiled.Events.Handlers.Player.Died += OnPlayerDeath;  // Fully qualified reference for Player.Died

        }

        public static void Unregister()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;  // Fully qualified reference for Server.RoundStarted
            Exiled.Events.Handlers.Player.Died -= OnPlayerDeath;  // Fully qualified reference for Player.Died
        }

        private static async void OnRoundStarted()
        {
            var tokenManager = Plugin.Instance?.VerificationManager;

            if (tokenManager == null)
            {
                Log.Warn("[SCPStats] TokenManager is null. Cannot verify server.");
                return;
            }

            bool isVerified = await tokenManager.AutoVerify();

            if (!isVerified)
            {
                Log.Error("[SCPStats] Server is NOT verified! Please wait.");
                return;
            }

            TimerHandler.RoundStarted();

            foreach (var player in Exiled.API.Features.Player.List)
            {
                if (!ShouldTrackPlayer(player))
                {
                    Log.Debug($"[SCPStats] Skipping stat registration for {player.Nickname} due to DNT or other conditions.");
                    continue;
                }

                PlayerStatsTracker.RegisterPlayer(player.UserId);
            }
        }

        private static void OnPlayerDeath(Exiled.Events.EventArgs.Player.DiedEventArgs ev)
        {
            var killer = ev.Attacker;

            if (killer != null && !ShouldTrackPlayer(killer))
            {
                Log.Debug($"[SCPStats] Skipping kill log for {killer.Nickname} due to DNT or other conditions.");
                return;
            }

            KillLogger.LogKill(ev);
        }

        /// <summary>
        /// Returns if stats related to a player should be recorded.
        /// </summary>
        /// <param name="ply">The player to check.</param>
        /// <returns>Boolean indicating whether stats should be recorded for the player.</returns>
        private static bool ShouldTrackPlayer(Exiled.API.Features.Player ply)
        {
            if (ply is null)
                return false;

            if (!ply.DoNotTrack)
                return true;

            Log.Debug($"[SCPStats] Player {ply.Nickname} excluded due to Do Not Track flag.");
            return false;
        }
    }
}
