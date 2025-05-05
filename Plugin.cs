using System;
using Exiled.API.Features;

namespace SCPStats
{
    public class Plugin : Plugin<Config>
    {
        public override string Name => "MySCPStats";
        public override string Author => "Joseph_fallen";
        public override Version Version => new(1, 2, 0);
        public static Plugin Instance { get; private set; }
        public TokenManager TokenManager { get; private set; }

        public override void OnEnabled()
        {
            // Ensure that the TokenManager is instantiated
            Instance = this;
            TokenManager = new TokenManager(); // Initializing TokenManager here

            // Register events and initialize timers
            EventHandler.Register();
            TimerHandler.Initialize();
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            // Clean up by disposing of timers and unregistering events
            TimerHandler.Dispose();
            EventHandler.Unregister();
            Instance = null;
            base.OnDisabled();
        }
    }
}
