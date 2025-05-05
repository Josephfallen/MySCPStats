using Exiled.API.Features;

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

        private static void OnRoundStarted()
        {
            if (Plugin.Instance?.TokenManager != null)
                _ = Plugin.Instance.TokenManager.FetchTokenAsync();
            else
                Log.Warn("[SCPStats] TokenManager is null. Token will not be fetched.");

            TimerHandler.RoundStarted();

            foreach (var player in Exiled.API.Features.Player.List)  // Use Exiled.API.Features.Player.List
            {
                if (!ShouldTrackPlayer(player))  // Call helper function to check if player should be tracked
                {
                    Log.Debug($"[SCPStats] Skipping stat registration for {player.Nickname} due to DNT or other conditions.");
                    continue;
                }

                PlayerStatsTracker.RegisterPlayer(player.UserId);
            }
        }

        private static void OnPlayerDeath(Exiled.Events.EventArgs.Player.DiedEventArgs ev)
        {
            var killer = ev.Attacker;  // Use Attacker instead of Killer

            if (killer != null && !ShouldTrackPlayer(killer))  // Call helper function to check if killer should be tracked
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

            bool shouldTrack = true;

            // Exclude players with DoNotTrack flag if configured
            if (Plugin.Instance.Config.ExcludeDNTUsers && ply.DoNotTrack)
            {
                shouldTrack = false;
                Log.Debug($"[SCPStats] Player {ply.Nickname} excluded due to Do Not Track flag.");
            }

            return shouldTrack;
        }

    }
}