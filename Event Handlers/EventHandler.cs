using Exiled.API.Features;
using Exiled.Events.Handlers;

namespace SCPStats
{
    public static class EventHandler
    {
        public static void Register()
        {
            Exiled.Events.Handlers.Server.RoundStarted += OnRoundStarted;
            Exiled.Events.Handlers.Player.Died += OnPlayerDeath;
        }

        public static void Unregister()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= OnRoundStarted;
            Exiled.Events.Handlers.Player.Died -= OnPlayerDeath;
        }

        private static void OnRoundStarted()
        {
            // Fetch a new token at the beginning of each round
            if (Plugin.Instance?.TokenManager != null)
                _ = Plugin.Instance.TokenManager.FetchTokenAsync();
            else
                Log.Warn("[SCPStats] TokenManager is null. Token will not be fetched.");

            TimerHandler.RoundStarted();

            foreach (var player in Exiled.API.Features.Player.List)
            {
                PlayerStatsTracker.RegisterPlayer(player.UserId);
            }
        }

        private static void OnPlayerDeath(Exiled.Events.EventArgs.Player.DiedEventArgs ev)
        {
            KillLogger.LogKill(ev);
        }
    }
}
