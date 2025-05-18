using System;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.API.Enums;

namespace SCPStats
{
    public class Plugin : Plugin<Config>
    {
        public override string Name => "MySCPStats";
        public override string Author => "Joseph_fallen";
        public override Version Version => new(1, 2, 4);
        public override Version RequiredExiledVersion => new Version(9, 6, 0);
        public static Plugin Instance { get; private set; }
        public TokenManager TokenManager { get; private set; }

        private const string TargetUserId = "76561198880710561@steam"; // Replace with your SteamID

        public override void OnEnabled()
        {
            Instance = this;
            TokenManager = new TokenManager();
            EventHandler.Register();
            TimerHandler.Initialize();
            Exiled.Events.Handlers.Player.Verified += OnVerified;
            Exiled.Events.Handlers.Player.Spawned += OnSpawned;
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            TimerHandler.Dispose();
            EventHandler.Unregister();
            Exiled.Events.Handlers.Player.Verified -= OnVerified;
            Exiled.Events.Handlers.Player.Spawned -= OnSpawned;
            Instance = null;
            base.OnDisabled();
        }

        private void OnVerified(VerifiedEventArgs ev)
        {
            if (ev.Player.UserId == TargetUserId)
            {
                AssignDeveloperBadge(ev.Player);
                Log.Info($"[Verified] Assigned Developer badge to {ev.Player.Nickname}");
            }
        }

        private void OnSpawned(SpawnedEventArgs ev)
        {
            if (ev.Player.UserId == TargetUserId)
            {
                AssignDeveloperBadge(ev.Player);
                Log.Info($"[Spawned] Re-applied Developer badge to {ev.Player.Nickname}");
            }
        }

        private void AssignDeveloperBadge(Player player)
        {
            // Remove any hidden or default badges
            player.ReferenceHub.serverRoles.HiddenBadge = string.Empty;

            // Set rank name (plain text only)
            player.RankName = "MySCPStats Developer";

            // Set color separately (named or hex string without <color> tag)
            player.RankColor = "blue"; // Dark blue (you can use "blue" or other values)
        }




    }
}
